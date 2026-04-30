using System.Security.Cryptography;
using System.Text.Json;
using AutoVhc.Api.Common;
using Npgsql;

namespace AutoVhc.Api.Infrastructure;

public sealed class PrototypeRepository
{
    private readonly string _connectionString;
    private readonly string _publicBaseUrl;
    private readonly string _contentRootPath;

    public PrototypeRepository(IConfiguration config, IWebHostEnvironment env)
    {
        _connectionString = config.GetConnectionString("Postgres")
            ?? "Host=localhost;Port=5432;Database=autovhc_prototype;Username=postgres;Password=postgres";
        _publicBaseUrl = config["Prototype:PublicBaseUrl"] ?? "http://localhost:5000";
        _contentRootPath = env.ContentRootPath;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var migrationPath = ResolveFilePath(
            "apps/db/migrations/001_init.sql",
            "../db/migrations/001_init.sql",
            "../../../../db/migrations/001_init.sql");
        var seedPath = ResolveFilePath(
            "apps/db/seed/001_seed.sql",
            "../db/seed/001_seed.sql",
            "../../../../db/seed/001_seed.sql");

        foreach (var path in new[] { migrationPath, seedPath })
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var sql = await File.ReadAllTextAsync(path, cancellationToken);
            if (string.IsNullOrWhiteSpace(sql))
            {
                continue;
            }

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private string? ResolveFilePath(params string[] relativeCandidates)
    {
        var roots = new[]
        {
            Directory.GetCurrentDirectory(),
            _contentRootPath,
            AppContext.BaseDirectory
        };

        foreach (var root in roots)
        {
            foreach (var relative in relativeCandidates)
            {
                var full = Path.GetFullPath(Path.Combine(root, relative));
                if (File.Exists(full))
                {
                    return full;
                }
            }
        }

        return null;
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var testConn = new NpgsqlConnection(_connectionString);
            await testConn.OpenAsync(cancellationToken);
            return;
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000")
        {
            // Missing database; create it from the postgres catalog DB.
        }

        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var targetDatabase = builder.Database;
        builder.Database = "postgres";

        await using var adminConn = new NpgsqlConnection(builder.ConnectionString);
        await adminConn.OpenAsync(cancellationToken);
        await using var createDb = new NpgsqlCommand($"CREATE DATABASE \"{targetDatabase}\";", adminConn);
        try
        {
            await createDb.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "42P04")
        {
            // Database already exists.
        }
    }

    public async Task<ImportResult> ImportFollowUpsAsync(
        ImportFollowUpRequest request,
        IReadOnlyList<MockFollowUpRecord> records,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var snapshotId = Guid.NewGuid();
        var correlationId = $"import-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var reqJson = JsonSerializer.Serialize(new
        {
            request.SiteCode,
            request.DueFrom,
            request.DueTo,
            request.RecordLimit,
            request.Offset,
            request.Order,
            request.LanguageId
        });

        await using (var snapshotCmd = new NpgsqlCommand(@"
INSERT INTO followup_import_snapshot(snapshot_id, site_code, request_filters, imported_at, record_count, correlation_id)
VALUES (@snapshot_id, @site_code, CAST(@request_filters AS jsonb), @imported_at, @record_count, @correlation_id);", conn))
        {
            snapshotCmd.Parameters.AddWithValue("snapshot_id", snapshotId);
            snapshotCmd.Parameters.AddWithValue("site_code", request.SiteCode);
            snapshotCmd.Parameters.AddWithValue("request_filters", reqJson);
            snapshotCmd.Parameters.AddWithValue("imported_at", DateTimeOffset.UtcNow);
            snapshotCmd.Parameters.AddWithValue("record_count", records.Count);
            snapshotCmd.Parameters.AddWithValue("correlation_id", correlationId);
            await snapshotCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var record in records)
        {
            var (eligibilityStatus, blockedReason) = await EvaluateEligibilityAsync(conn, record, cancellationToken);

            await using var upsertCmd = new NpgsqlCommand(@"
INSERT INTO followup_event (
  followup_item_id, event_type, site_code, customer_id, customer_name, mobile_number, email_address, preferred_channel,
  vehicle_registration, vehicle_make_model, followup_description, original_inspection_date, due_date, item_status,
  disable_follow_up, estimated_value, language_code, language_id, eligibility_status, blocked_reason, imported_at, updated_at)
VALUES (
  @followup_item_id, @event_type, @site_code, @customer_id, @customer_name, @mobile_number, @email_address, @preferred_channel,
  @vehicle_registration, @vehicle_make_model, @followup_description, @original_inspection_date, @due_date, @item_status,
  @disable_follow_up, @estimated_value, @language_code, @language_id, @eligibility_status, @blocked_reason, @imported_at, @updated_at)
ON CONFLICT (followup_item_id) DO UPDATE
SET event_type = EXCLUDED.event_type,
    site_code = EXCLUDED.site_code,
    customer_id = EXCLUDED.customer_id,
    customer_name = EXCLUDED.customer_name,
    mobile_number = EXCLUDED.mobile_number,
    email_address = EXCLUDED.email_address,
    preferred_channel = EXCLUDED.preferred_channel,
    vehicle_registration = EXCLUDED.vehicle_registration,
    vehicle_make_model = EXCLUDED.vehicle_make_model,
    followup_description = EXCLUDED.followup_description,
    original_inspection_date = EXCLUDED.original_inspection_date,
    due_date = EXCLUDED.due_date,
    item_status = EXCLUDED.item_status,
    disable_follow_up = EXCLUDED.disable_follow_up,
    estimated_value = EXCLUDED.estimated_value,
    language_code = EXCLUDED.language_code,
    language_id = EXCLUDED.language_id,
    eligibility_status = EXCLUDED.eligibility_status,
    blocked_reason = EXCLUDED.blocked_reason,
    imported_at = EXCLUDED.imported_at,
    updated_at = EXCLUDED.updated_at;", conn);

            upsertCmd.Parameters.AddWithValue("followup_item_id", record.FollowUpItemId);
            upsertCmd.Parameters.AddWithValue("event_type", record.EventType);
            upsertCmd.Parameters.AddWithValue("site_code", record.SiteCode);
            upsertCmd.Parameters.AddWithValue("customer_id", record.CustomerId);
            upsertCmd.Parameters.AddWithValue("customer_name", record.CustomerName);
            upsertCmd.Parameters.AddWithValue("mobile_number", (object?)record.MobileNumber ?? DBNull.Value);
            upsertCmd.Parameters.AddWithValue("email_address", (object?)record.EmailAddress ?? DBNull.Value);
            upsertCmd.Parameters.AddWithValue("preferred_channel", (object?)record.PreferredChannel ?? DBNull.Value);
            upsertCmd.Parameters.AddWithValue("vehicle_registration", record.VehicleRegistration);
            upsertCmd.Parameters.AddWithValue("vehicle_make_model", record.VehicleMakeModel);
            upsertCmd.Parameters.AddWithValue("followup_description", record.FollowUpDescription);
            upsertCmd.Parameters.AddWithValue("original_inspection_date", record.OriginalInspectionDate.ToDateTime(TimeOnly.MinValue));
            upsertCmd.Parameters.AddWithValue("due_date", record.DueDate.ToDateTime(TimeOnly.MinValue));
            upsertCmd.Parameters.AddWithValue("item_status", record.ItemStatus);
            upsertCmd.Parameters.AddWithValue("disable_follow_up", record.DisableFollowUp);
            upsertCmd.Parameters.AddWithValue("estimated_value", record.EstimatedValue);
            upsertCmd.Parameters.AddWithValue("language_code", record.LanguageCode);
            upsertCmd.Parameters.AddWithValue("language_id", record.LanguageId);
            upsertCmd.Parameters.AddWithValue("eligibility_status", eligibilityStatus);
            upsertCmd.Parameters.AddWithValue("blocked_reason", (object?)blockedReason ?? DBNull.Value);
            upsertCmd.Parameters.AddWithValue("imported_at", DateTimeOffset.UtcNow);
            upsertCmd.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);

            await upsertCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return new ImportResult(snapshotId, correlationId, records.Count);
    }

    public async Task<List<Dictionary<string, object?>>> GetEligibleAsync(string? siteCode, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = siteCode is null or ""
            ? @"
SELECT followup_item_id, event_type, site_code, customer_name, vehicle_registration, vehicle_make_model,
       followup_description, due_date, preferred_channel, eligibility_status, blocked_reason, item_status,
       disable_follow_up, estimated_value
FROM followup_event
ORDER BY due_date ASC, followup_item_id;"
            : @"
SELECT followup_item_id, event_type, site_code, customer_name, vehicle_registration, vehicle_make_model,
       followup_description, due_date, preferred_channel, eligibility_status, blocked_reason, item_status,
       disable_follow_up, estimated_value
FROM followup_event
WHERE site_code = @site_code
ORDER BY due_date ASC, followup_item_id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(siteCode))
        {
            cmd.Parameters.AddWithValue("site_code", siteCode);
        }

        var result = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Dictionary<string, object?>
            {
                ["followUpItemId"] = reader.GetString(0),
                ["eventType"] = reader.GetString(1),
                ["siteCode"] = reader.GetString(2),
                ["customerName"] = reader.GetString(3),
                ["vehicleRegistration"] = reader.GetString(4),
                ["vehicleMakeModel"] = reader.GetString(5),
                ["followUpDescription"] = reader.GetString(6),
                ["dueDate"] = reader.GetDateTime(7).Date,
                ["preferredChannel"] = reader.IsDBNull(8) ? null : reader.GetString(8),
                ["eligibilityStatus"] = reader.GetString(9),
                ["blockedReason"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                ["itemStatus"] = reader.GetString(11),
                ["disableFollowUp"] = reader.GetBoolean(12),
                ["estimatedValue"] = reader.IsDBNull(13) ? null : reader.GetDecimal(13)
            });
        }

        return result;
    }

    public async Task<List<Dictionary<string, object?>>> GetActivityAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = @"
SELECT fe.followup_item_id, fe.customer_name, fe.vehicle_registration, fe.event_type,
       rm.channel, rm.sent_at, rm.opened_at,
       aa.customer_action, aa.status, aa.assigned_to
FROM followup_event fe
LEFT JOIN reminder_message rm ON rm.followup_item_id = fe.followup_item_id
LEFT JOIN advisor_action aa ON aa.followup_item_id = fe.followup_item_id
ORDER BY COALESCE(rm.sent_at, fe.updated_at) DESC NULLS LAST;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        var result = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Dictionary<string, object?>
            {
                ["followUpItemId"] = reader.GetString(0),
                ["customerName"] = reader.GetString(1),
                ["vehicleRegistration"] = reader.GetString(2),
                ["eventType"] = reader.GetString(3),
                ["channel"] = reader.IsDBNull(4) ? null : reader.GetString(4),
                ["sentAt"] = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                ["openedAt"] = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ["customerAction"] = reader.IsDBNull(7) ? null : reader.GetString(7),
                ["status"] = reader.IsDBNull(8) ? null : reader.GetString(8),
                ["assignedTo"] = reader.IsDBNull(9) ? null : reader.GetString(9)
            });
        }

        return result;
    }

    public async Task<List<Dictionary<string, object?>>> CreateReminderMessagesAsync(
        CreateRemindersRequest request,
        CancellationToken cancellationToken)
    {
        var created = new List<Dictionary<string, object?>>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        foreach (var followUpItemId in request.FollowUpItemIds.Distinct())
        {
            var followUp = await GetFollowUpByIdAsync(conn, followUpItemId, cancellationToken);
            if (followUp is null)
            {
                continue;
            }

            if (!string.Equals(followUp.EligibilityStatus, "eligible", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var template = await GetTemplateAsync(conn, followUp.EventType, followUp.PreferredChannel, followUp.LanguageCode, cancellationToken);
            if (template is null)
            {
                continue;
            }

            var messageId = Guid.NewGuid();
            var token = CreateTrackingToken();
            var now = DateTimeOffset.UtcNow;

            await using var insert = new NpgsqlCommand(@"
INSERT INTO reminder_message(
  message_id, followup_item_id, event_type, provider, provider_message_id, channel,
  template_key, template_version, language_code, tracking_token, status, expires_at, created_at)
VALUES(
  @message_id, @followup_item_id, @event_type, 'infobip', NULL, @channel,
  @template_key, @template_version, @language_code, @tracking_token, 'created', @expires_at, @created_at);", conn);

            insert.Parameters.AddWithValue("message_id", messageId);
            insert.Parameters.AddWithValue("followup_item_id", followUpItemId);
            insert.Parameters.AddWithValue("event_type", followUp.EventType);
            insert.Parameters.AddWithValue("channel", followUp.PreferredChannel);
            insert.Parameters.AddWithValue("template_key", template.TranslationKey);
            insert.Parameters.AddWithValue("template_version", template.Version);
            insert.Parameters.AddWithValue("language_code", followUp.LanguageCode);
            insert.Parameters.AddWithValue("tracking_token", token);
            insert.Parameters.AddWithValue("expires_at", now.AddDays(30));
            insert.Parameters.AddWithValue("created_at", now);
            await insert.ExecuteNonQueryAsync(cancellationToken);

            created.Add(new Dictionary<string, object?>
            {
                ["messageId"] = messageId,
                ["followUpItemId"] = followUpItemId,
                ["trackingToken"] = token,
                ["status"] = "created"
            });
        }

        return created;
    }

    public async Task<int> SendMockAsync(SendMockRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var ids = request.MessageIds?.Distinct().ToList();
        var now = DateTimeOffset.UtcNow;

        var sql = ids is { Count: > 0 }
            ? @"
UPDATE reminder_message
SET status='delivered', provider_message_id = CONCAT('mock-', substr(gen_random_uuid()::text, 1, 8)),
    sent_at=@sent_at, delivered_at=@delivered_at
WHERE message_id = ANY(@message_ids::uuid[]);"
            : @"
UPDATE reminder_message
SET status='delivered', provider_message_id = CONCAT('mock-', substr(gen_random_uuid()::text, 1, 8)),
    sent_at=COALESCE(sent_at, @sent_at), delivered_at=COALESCE(delivered_at, @delivered_at)
WHERE status='created';";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("sent_at", now);
        cmd.Parameters.AddWithValue("delivered_at", now.AddSeconds(2));
        if (ids is { Count: > 0 })
        {
            cmd.Parameters.AddWithValue("message_ids", ids);
        }

        var updated = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return updated;
    }

    public async Task<List<Dictionary<string, object?>>> GetOutboxAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = @"
SELECT rm.message_id, rm.followup_item_id, rm.status, rm.channel, rm.tracking_token, rm.sent_at,
       fe.customer_name, fe.vehicle_registration, fe.followup_description
FROM reminder_message rm
JOIN followup_event fe ON fe.followup_item_id = rm.followup_item_id
ORDER BY rm.created_at DESC;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var rows = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var token = reader.GetString(4);
            rows.Add(new Dictionary<string, object?>
            {
                ["messageId"] = reader.GetGuid(0),
                ["followUpItemId"] = reader.GetString(1),
                ["status"] = reader.GetString(2),
                ["channel"] = reader.GetString(3),
                ["trackingToken"] = token,
                ["personalisedUrl"] = $"{_publicBaseUrl.TrimEnd('/')}/r/{token}",
                ["sentAt"] = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                ["customerName"] = reader.GetString(6),
                ["vehicleRegistration"] = reader.GetString(7),
                ["followUpDescription"] = reader.GetString(8)
            });
        }

        return rows;
    }

    public async Task<Dictionary<string, object?>?> GetReminderLandingAsync(string trackingToken, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = @"
SELECT rm.message_id, rm.expires_at, fe.followup_item_id, fe.customer_name, fe.vehicle_registration,
       fe.vehicle_make_model, fe.followup_description, fe.event_type, fe.original_inspection_date,
       dsc.dealer_name, dsc.phone, dsc.email, dsc.address, dsc.opening_hours
FROM reminder_message rm
JOIN followup_event fe ON fe.followup_item_id = rm.followup_item_id
JOIN dealer_site_config dsc ON dsc.site_code = fe.site_code
WHERE rm.tracking_token = @tracking_token;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("tracking_token", trackingToken);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var messageId = reader.GetGuid(0);
        var expiresAt = reader.GetFieldValue<DateTimeOffset>(1);
        if (expiresAt < DateTimeOffset.UtcNow)
        {
            return new Dictionary<string, object?>
            {
                ["expired"] = true,
                ["message"] = "This reminder link has expired."
            };
        }

        await reader.CloseAsync();
        await MarkOpenedAsync(conn, messageId, cancellationToken);

        await using var cmd2 = new NpgsqlCommand(sql, conn);
        cmd2.Parameters.AddWithValue("tracking_token", trackingToken);
        await using var read2 = await cmd2.ExecuteReaderAsync(cancellationToken);
        await read2.ReadAsync(cancellationToken);

        return new Dictionary<string, object?>
        {
            ["expired"] = false,
            ["followUpItemId"] = read2.GetString(2),
            ["customerName"] = read2.GetString(3),
            ["vehicleRegistration"] = read2.GetString(4),
            ["vehicleMakeModel"] = read2.GetString(5),
            ["followUpDescription"] = read2.GetString(6),
            ["eventType"] = read2.GetString(7),
            ["originalInspectionDate"] = read2.IsDBNull(8) ? null : DateOnly.FromDateTime(read2.GetDateTime(8)).ToString("dd MMM yyyy"),
            ["dealer"] = new Dictionary<string, object?>
            {
                ["name"] = read2.GetString(9),
                ["phone"] = read2.GetString(10),
                ["email"] = read2.GetString(11),
                ["address"] = read2.GetString(12),
                ["openingHours"] = read2.GetString(13)
            }
        };
    }

    public async Task<bool> RecordCallbackAsync(string trackingToken, CallbackRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var message = await GetMessageByTokenAsync(conn, trackingToken, cancellationToken);
        if (message is null)
        {
            return false;
        }

        await TrackAsync(conn, message.MessageId, "callback_requested", new { request.PreferredTime, request.PhoneNumber, request.Note }, cancellationToken);

        await using (var updateMessage = new NpgsqlCommand(@"
UPDATE reminder_message SET clicked_at = COALESCE(clicked_at, @clicked_at), status = 'clicked' WHERE message_id = @message_id;", conn))
        {
            updateMessage.Parameters.AddWithValue("clicked_at", DateTimeOffset.UtcNow);
            updateMessage.Parameters.AddWithValue("message_id", message.MessageId);
            await updateMessage.ExecuteNonQueryAsync(cancellationToken);
        }

        await EnsureAdvisorActionAsync(conn, message.FollowUpItemId, "callback_requested", cancellationToken);
        return true;
    }

    public async Task<bool> RecordCallDealerClickAsync(string trackingToken, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var message = await GetMessageByTokenAsync(conn, trackingToken, cancellationToken);
        if (message is null)
        {
            return false;
        }

        await TrackAsync(conn, message.MessageId, "call_dealer_clicked", new { source = "customer_page" }, cancellationToken);
        await EnsureAdvisorActionAsync(conn, message.FollowUpItemId, "call_dealer_clicked", cancellationToken, createIfMissing: false);
        return true;
    }

    public async Task<bool> RecordRemindLaterAsync(string trackingToken, RemindLaterRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var message = await GetMessageByTokenAsync(conn, trackingToken, cancellationToken);
        if (message is null)
        {
            return false;
        }

        await TrackAsync(conn, message.MessageId, "remind_later", new { request.Days, request.Note }, cancellationToken);

        await using var update = new NpgsqlCommand(@"
UPDATE followup_event
SET due_date = due_date + @days,
    eligibility_status = 'eligible',
    blocked_reason = NULL,
    updated_at = @updated_at
WHERE followup_item_id = @followup_item_id;", conn);
        update.Parameters.AddWithValue("days", request.Days);
        update.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);
        update.Parameters.AddWithValue("followup_item_id", message.FollowUpItemId);
        await update.ExecuteNonQueryAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RecordAlreadyRepairedAsync(string trackingToken, AlreadyRepairedRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var message = await GetMessageByTokenAsync(conn, trackingToken, cancellationToken);
        if (message is null)
        {
            return false;
        }

        await TrackAsync(conn, message.MessageId, "already_repaired", new { request.Note }, cancellationToken);

        await using var update = new NpgsqlCommand(@"
UPDATE followup_event
SET item_status = 'closed', eligibility_status = 'suppressed', blocked_reason = 'already_repaired', updated_at = @updated_at
WHERE followup_item_id = @followup_item_id;", conn);
        update.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);
        update.Parameters.AddWithValue("followup_item_id", message.FollowUpItemId);
        await update.ExecuteNonQueryAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RecordStopAsync(string trackingToken, StopRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var message = await GetMessageByTokenAsync(conn, trackingToken, cancellationToken);
        if (message is null)
        {
            return false;
        }

        await TrackAsync(conn, message.MessageId, "stop_reminders", new { request.Channel, request.Reason }, cancellationToken);

        var customerId = await GetCustomerIdByFollowUpAsync(conn, message.FollowUpItemId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            await using var insertSuppression = new NpgsqlCommand(@"
INSERT INTO suppression_preference(suppression_id, customer_id, channel, opt_out_scope, source, reason, created_at)
VALUES(@suppression_id, @customer_id, @channel, 'all', 'customer_cta', @reason, @created_at);", conn);

            insertSuppression.Parameters.AddWithValue("suppression_id", Guid.NewGuid());
            insertSuppression.Parameters.AddWithValue("customer_id", customerId);
            insertSuppression.Parameters.AddWithValue("channel", (object?)request.Channel ?? DBNull.Value);
            insertSuppression.Parameters.AddWithValue("reason", (object?)request.Reason ?? DBNull.Value);
            insertSuppression.Parameters.AddWithValue("created_at", DateTimeOffset.UtcNow);
            await insertSuppression.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var update = new NpgsqlCommand(@"
UPDATE followup_event
SET eligibility_status = 'suppressed', blocked_reason = 'customer_opt_out', updated_at = @updated_at
WHERE followup_item_id = @followup_item_id;", conn);
        update.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);
        update.Parameters.AddWithValue("followup_item_id", message.FollowUpItemId);
        await update.ExecuteNonQueryAsync(cancellationToken);

        return true;
    }

    public async Task<List<Dictionary<string, object?>>> GetAdvisorActionsAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var sql = @"
SELECT aa.action_id, aa.followup_item_id, aa.customer_action, aa.status, aa.assigned_to, aa.callback_requested_at,
       aa.call_made_at, aa.sla_deadline, aa.outcome, fe.customer_name, fe.vehicle_registration, fe.followup_description
FROM advisor_action aa
JOIN followup_event fe ON fe.followup_item_id = aa.followup_item_id
ORDER BY aa.sla_deadline ASC;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var rows = new List<Dictionary<string, object?>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var slaDeadline = reader.GetFieldValue<DateTimeOffset>(7);
            rows.Add(new Dictionary<string, object?>
            {
                ["actionId"] = reader.GetGuid(0),
                ["followUpItemId"] = reader.GetString(1),
                ["customerAction"] = reader.GetString(2),
                ["status"] = reader.GetString(3),
                ["assignedTo"] = reader.IsDBNull(4) ? null : reader.GetString(4),
                ["callbackRequestedAt"] = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                ["callMadeAt"] = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ["slaDeadline"] = slaDeadline,
                ["isOverdue"] = slaDeadline < DateTimeOffset.UtcNow && reader.GetString(3) != "closed",
                ["outcome"] = reader.IsDBNull(8) ? null : reader.GetString(8),
                ["customerName"] = reader.GetString(9),
                ["vehicleRegistration"] = reader.GetString(10),
                ["followUpDescription"] = reader.GetString(11)
            });
        }

        return rows;
    }

    public async Task<bool> AssignAdvisorActionAsync(Guid actionId, AssignActionRequest request, CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
UPDATE advisor_action
SET assigned_to = @assigned_to,
    status = CASE WHEN status='new' THEN 'assigned' ELSE status END
WHERE action_id = @action_id;", conn);
        cmd.Parameters.AddWithValue("assigned_to", request.AssignedTo);
        cmd.Parameters.AddWithValue("action_id", actionId);

        var updated = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return updated > 0;
    }

    public async Task<bool> CloseAdvisorActionAsync(Guid actionId, CloseActionRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Outcome))
        {
            return false;
        }

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        string? followUpItemId = null;
        await using (var lookup = new NpgsqlCommand("SELECT followup_item_id FROM advisor_action WHERE action_id = @action_id;", conn))
        {
            lookup.Parameters.AddWithValue("action_id", actionId);
            followUpItemId = (string?)await lookup.ExecuteScalarAsync(cancellationToken);
        }

        if (followUpItemId is null)
        {
            return false;
        }

        await using (var close = new NpgsqlCommand(@"
UPDATE advisor_action
SET status='closed', outcome=@outcome, notes=@notes, closed_at=@closed_at,
    call_made_at = COALESCE(call_made_at, @call_made_at)
WHERE action_id=@action_id;", conn))
        {
            close.Parameters.AddWithValue("outcome", request.Outcome);
            close.Parameters.AddWithValue("notes", (object?)request.Notes ?? DBNull.Value);
            close.Parameters.AddWithValue("closed_at", DateTimeOffset.UtcNow);
            close.Parameters.AddWithValue("call_made_at", DateTimeOffset.UtcNow);
            close.Parameters.AddWithValue("action_id", actionId);
            await close.ExecuteNonQueryAsync(cancellationToken);
        }

        var mappedStatus = request.Outcome.Equals("booked", StringComparison.OrdinalIgnoreCase) ? "booked" : "closed";
        await using (var followUpUpdate = new NpgsqlCommand(@"
UPDATE followup_event
SET item_status=@item_status,
    eligibility_status = CASE WHEN @item_status = 'booked' THEN 'suppressed' ELSE eligibility_status END,
    blocked_reason = CASE WHEN @item_status = 'booked' THEN 'booked' ELSE blocked_reason END,
    updated_at=@updated_at
WHERE followup_item_id=@followup_item_id;", conn))
        {
            followUpUpdate.Parameters.AddWithValue("item_status", mappedStatus);
            followUpUpdate.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);
            followUpUpdate.Parameters.AddWithValue("followup_item_id", followUpItemId);
            await followUpUpdate.ExecuteNonQueryAsync(cancellationToken);
        }

        if (request.StopFurtherReminders)
        {
            await using var suppress = new NpgsqlCommand(@"
UPDATE followup_event
SET eligibility_status='suppressed', blocked_reason='closed_by_advisor', updated_at=@updated_at
WHERE followup_item_id=@followup_item_id;", conn);
            suppress.Parameters.AddWithValue("updated_at", DateTimeOffset.UtcNow);
            suppress.Parameters.AddWithValue("followup_item_id", followUpItemId);
            await suppress.ExecuteNonQueryAsync(cancellationToken);
        }

        return true;
    }

    public async Task<Dictionary<string, object?>> GetFunnelReportAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var report = new Dictionary<string, object?>();

        report["imported"] = await ScalarIntAsync(conn, "SELECT COALESCE(SUM(record_count),0) FROM followup_import_snapshot;", cancellationToken);
        report["eligible"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM followup_event WHERE eligibility_status='eligible';", cancellationToken);
        report["blocked"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM followup_event WHERE eligibility_status='blocked';", cancellationToken);
        report["suppressed"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM followup_event WHERE eligibility_status='suppressed';", cancellationToken);
        report["sent"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM reminder_message WHERE sent_at IS NOT NULL;", cancellationToken);
        report["delivered"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM reminder_message WHERE delivered_at IS NOT NULL;", cancellationToken);
        report["opened"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM reminder_message WHERE opened_at IS NOT NULL;", cancellationToken);
        report["clicked"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM reminder_message WHERE clicked_at IS NOT NULL;", cancellationToken);
        report["callbackRequested"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE customer_action='callback_requested';", cancellationToken);
        report["booked"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE outcome='booked';", cancellationToken);
        report["closedLost"] = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE status='closed' AND COALESCE(outcome,'') <> 'booked';", cancellationToken);

        return report;
    }

    public async Task<List<Dictionary<string, object?>>> GetBlockedReasonsAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
SELECT COALESCE(blocked_reason, 'none') AS blocked_reason, COUNT(*)
FROM followup_event
WHERE eligibility_status IN ('blocked', 'suppressed')
GROUP BY COALESCE(blocked_reason, 'none')
ORDER BY COUNT(*) DESC;", conn);

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["blockedReason"] = reader.GetString(0),
                ["count"] = reader.GetInt64(1)
            });
        }

        return rows;
    }

    public async Task<Dictionary<string, object?>> GetSlaReportAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var newCount = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE status='new';", cancellationToken);
        var assignedCount = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE status='assigned';", cancellationToken);
        var closedCount = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE status='closed';", cancellationToken);
        var overdueCount = await ScalarIntAsync(conn, "SELECT COUNT(*) FROM advisor_action WHERE status <> 'closed' AND sla_deadline < now();", cancellationToken);

        await using var avgCmd = new NpgsqlCommand(@"
SELECT AVG(EXTRACT(EPOCH FROM (COALESCE(call_made_at, now()) - callback_requested_at)))
FROM advisor_action
WHERE callback_requested_at IS NOT NULL;", conn);
        var avgSecondsObj = await avgCmd.ExecuteScalarAsync(cancellationToken);
        var avgSeconds = avgSecondsObj is null || avgSecondsObj == DBNull.Value ? 0d : Convert.ToDouble(avgSecondsObj);

        return new Dictionary<string, object?>
        {
            ["new"] = newCount,
            ["assigned"] = assignedCount,
            ["closed"] = closedCount,
            ["overdue"] = overdueCount,
            ["averageResponseMinutes"] = Math.Round(avgSeconds / 60d, 2)
        };
    }

    public async Task<List<Dictionary<string, object?>>> GetOpportunity7DayAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
SELECT event_type, COUNT(*) AS due_count, COALESCE(SUM(estimated_value),0) AS total_value
FROM followup_event
WHERE due_date BETWEEN current_date AND current_date + INTERVAL '7 days'
GROUP BY event_type
ORDER BY event_type;", conn);

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["eventType"] = reader.GetString(0),
                ["dueCount"] = reader.GetInt64(1),
                ["estimatedValue"] = reader.GetDecimal(2)
            });
        }

        return rows;
    }

    private static async Task<int> ScalarIntAsync(NpgsqlConnection conn, string sql, CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }

    private static string CreateTrackingToken()
    {
        Span<byte> bytes = stackalloc byte[18];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task<(string EligibilityStatus, string? BlockedReason)> EvaluateEligibilityAsync(
        NpgsqlConnection conn,
        MockFollowUpRecord record,
        CancellationToken cancellationToken)
    {
        if (record.DisableFollowUp)
        {
            return ("suppressed", "disable_follow_up_true");
        }

        if (record.DueDate > DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            return ("blocked", "not_due_yet");
        }

        if (string.IsNullOrWhiteSpace(record.MobileNumber) && string.IsNullOrWhiteSpace(record.EmailAddress))
        {
            return ("blocked", "no_contact_channel");
        }

        if (string.IsNullOrWhiteSpace(record.PreferredChannel))
        {
            return ("blocked", "missing_preferred_channel");
        }

        if (await IsSuppressedAsync(conn, record.CustomerId, record.PreferredChannel, cancellationToken))
        {
            return ("suppressed", "customer_opt_out");
        }

        return ("eligible", null);
    }

    private static async Task<bool> IsSuppressedAsync(
        NpgsqlConnection conn,
        string customerId,
        string channel,
        CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
SELECT 1
FROM suppression_preference
WHERE customer_id = @customer_id
  AND (channel IS NULL OR channel = @channel)
LIMIT 1;", conn);
        cmd.Parameters.AddWithValue("customer_id", customerId);
        cmd.Parameters.AddWithValue("channel", channel);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    private static async Task<FollowUpRow?> GetFollowUpByIdAsync(NpgsqlConnection conn, string followUpItemId, CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
SELECT followup_item_id, event_type, COALESCE(preferred_channel, 'sms'),
       COALESCE(language_code, 'en-GB'), eligibility_status
FROM followup_event
WHERE followup_item_id = @followup_item_id;", conn);
        cmd.Parameters.AddWithValue("followup_item_id", followUpItemId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new FollowUpRow(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4));
    }

    private static async Task<TemplateRow?> GetTemplateAsync(
        NpgsqlConnection conn,
        string eventType,
        string channel,
        string languageCode,
        CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
SELECT translation_key, version
FROM reminder_template
WHERE event_type = @event_type
  AND channel = @channel
  AND language_code = @language_code
  AND approval_status = 'approved'
ORDER BY version DESC
LIMIT 1;", conn);

        cmd.Parameters.AddWithValue("event_type", eventType);
        cmd.Parameters.AddWithValue("channel", channel);
        cmd.Parameters.AddWithValue("language_code", languageCode);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new TemplateRow(reader.GetString(0), reader.GetInt32(1));
    }

    private static async Task<MessageLookup?> GetMessageByTokenAsync(
        NpgsqlConnection conn,
        string trackingToken,
        CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
SELECT message_id, followup_item_id
FROM reminder_message
WHERE tracking_token = @tracking_token;", conn);
        cmd.Parameters.AddWithValue("tracking_token", trackingToken);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new MessageLookup(reader.GetGuid(0), reader.GetString(1));
    }

    private static async Task<string?> GetCustomerIdByFollowUpAsync(
        NpgsqlConnection conn,
        string followUpItemId,
        CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
SELECT customer_id
FROM followup_event
WHERE followup_item_id = @followup_item_id;", conn);
        cmd.Parameters.AddWithValue("followup_item_id", followUpItemId);

        var value = await cmd.ExecuteScalarAsync(cancellationToken);
        return value is null || value == DBNull.Value ? null : (string)value;
    }

    private static async Task TrackAsync(
        NpgsqlConnection conn,
        Guid messageId,
        string eventType,
        object details,
        CancellationToken cancellationToken)
    {
        await using var cmd = new NpgsqlCommand(@"
INSERT INTO tracking_event(event_id, message_id, event_type, event_timestamp, details, user_agent)
VALUES(@event_id, @message_id, @event_type, @event_timestamp, CAST(@details AS jsonb), @user_agent);", conn);

        cmd.Parameters.AddWithValue("event_id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("message_id", messageId);
        cmd.Parameters.AddWithValue("event_type", eventType);
        cmd.Parameters.AddWithValue("event_timestamp", DateTimeOffset.UtcNow);
        cmd.Parameters.AddWithValue("details", JsonSerializer.Serialize(details));
        cmd.Parameters.AddWithValue("user_agent", "prototype");
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task MarkOpenedAsync(NpgsqlConnection conn, Guid messageId, CancellationToken cancellationToken)
    {
        await using (var updateMessage = new NpgsqlCommand(@"
UPDATE reminder_message
SET opened_at = COALESCE(opened_at, @opened_at),
    status = CASE WHEN status IN ('created','delivered','sent') THEN 'opened' ELSE status END
WHERE message_id = @message_id;", conn))
        {
            updateMessage.Parameters.AddWithValue("opened_at", DateTimeOffset.UtcNow);
            updateMessage.Parameters.AddWithValue("message_id", messageId);
            await updateMessage.ExecuteNonQueryAsync(cancellationToken);
        }

        await TrackAsync(conn, messageId, "opened", new { source = "public_link" }, cancellationToken);
    }

    private static async Task EnsureAdvisorActionAsync(
        NpgsqlConnection conn,
        string followUpItemId,
        string customerAction,
        CancellationToken cancellationToken,
        bool createIfMissing = true)
    {
        await using var existsCmd = new NpgsqlCommand(@"
SELECT action_id
FROM advisor_action
WHERE followup_item_id = @followup_item_id
  AND status <> 'closed'
ORDER BY callback_requested_at DESC NULLS LAST
LIMIT 1;", conn);
        existsCmd.Parameters.AddWithValue("followup_item_id", followUpItemId);
        var existing = await existsCmd.ExecuteScalarAsync(cancellationToken);
        if (existing is not null)
        {
            return;
        }

        if (!createIfMissing)
        {
            return;
        }

        await using var insertCmd = new NpgsqlCommand(@"
INSERT INTO advisor_action(
  action_id, followup_item_id, customer_action, status, assigned_to,
  callback_requested_at, call_made_at, sla_deadline, outcome, notes, closed_at)
VALUES(
  @action_id, @followup_item_id, @customer_action, 'new', NULL,
  @callback_requested_at, NULL, @sla_deadline, NULL, NULL, NULL);", conn);

        insertCmd.Parameters.AddWithValue("action_id", Guid.NewGuid());
        insertCmd.Parameters.AddWithValue("followup_item_id", followUpItemId);
        insertCmd.Parameters.AddWithValue("customer_action", customerAction);
        insertCmd.Parameters.AddWithValue("callback_requested_at", DateTimeOffset.UtcNow);
        insertCmd.Parameters.AddWithValue("sla_deadline", DateTimeOffset.UtcNow.AddHours(2));
        await insertCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record FollowUpRow(
        string FollowUpItemId,
        string EventType,
        string PreferredChannel,
        string LanguageCode,
        string EligibilityStatus);

    private sealed record TemplateRow(string TranslationKey, int Version);
    private sealed record MessageLookup(Guid MessageId, string FollowUpItemId);
}

public sealed record ImportResult(Guid SnapshotId, string CorrelationId, int ImportedCount);
