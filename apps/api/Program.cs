using AutoVhc.Api.Common;
using AutoVhc.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddSingleton<MockFollowUpSource>();
builder.Services.AddSingleton<PrototypeRepository>();

var app = builder.Build();

var repo = app.Services.GetRequiredService<PrototypeRepository>();
await repo.InitializeAsync(CancellationToken.None);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("web");

app.MapGet("/", () => Results.Ok(new
{
    name = "autoVHC Automated Follow-Up Prototype API",
    version = "v0.1",
    utcNow = DateTimeOffset.UtcNow
}));

app.MapPost("/api/followup/import", async (
    ImportFollowUpRequest request,
    MockFollowUpSource source,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var records = source.GetRecords(request.SiteCode);
    var result = await repository.ImportFollowUpsAsync(request, records, cancellationToken);

    return Results.Ok(new
    {
        result.SnapshotId,
        result.CorrelationId,
        result.ImportedCount,
        siteCode = request.SiteCode
    });
});

app.MapGet("/api/followup/eligible", async (
    string? siteCode,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var rows = await repository.GetEligibleAsync(siteCode, cancellationToken);
    return Results.Ok(rows);
});

app.MapGet("/api/followup/activity", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var rows = await repository.GetActivityAsync(cancellationToken);
    return Results.Ok(rows);
});

app.MapPost("/api/reminders/create", async (
    CreateRemindersRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    if (request.FollowUpItemIds.Count == 0)
    {
        return Results.BadRequest(new { error = "followUpItemIds is required" });
    }

    var created = await repository.CreateReminderMessagesAsync(request, cancellationToken);
    return Results.Ok(new { createdCount = created.Count, messages = created });
});

app.MapPost("/api/reminders/send-mock", async (
    SendMockRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var sent = await repository.SendMockAsync(request, cancellationToken);
    return Results.Ok(new { sent });
});

app.MapGet("/api/reminders/outbox", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var outbox = await repository.GetOutboxAsync(cancellationToken);
    return Results.Ok(outbox);
});

app.MapGet("/r/{trackingToken}", async (
    string trackingToken,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var reminder = await repository.GetReminderLandingAsync(trackingToken, cancellationToken);
    return reminder is null ? Results.NotFound(new { error = "tracking token not found" }) : Results.Ok(reminder);
});

app.MapPost("/r/{trackingToken}/callback", async (
    string trackingToken,
    CallbackRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var ok = await repository.RecordCallbackAsync(trackingToken, request, cancellationToken);
    return ok ? Results.Ok(new { status = "callback_requested" }) : Results.NotFound(new { error = "tracking token not found" });
});

app.MapPost("/r/{trackingToken}/call-dealer-click", async (
    string trackingToken,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var ok = await repository.RecordCallDealerClickAsync(trackingToken, cancellationToken);
    return ok ? Results.Ok(new { status = "call_dealer_clicked" }) : Results.NotFound(new { error = "tracking token not found" });
});

app.MapPost("/r/{trackingToken}/remind-later", async (
    string trackingToken,
    RemindLaterRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var ok = await repository.RecordRemindLaterAsync(trackingToken, request, cancellationToken);
    return ok ? Results.Ok(new { status = "remind_later_saved", request.Days }) : Results.NotFound(new { error = "tracking token not found" });
});

app.MapPost("/r/{trackingToken}/already-repaired", async (
    string trackingToken,
    AlreadyRepairedRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var ok = await repository.RecordAlreadyRepairedAsync(trackingToken, request, cancellationToken);
    return ok ? Results.Ok(new { status = "already_repaired_recorded" }) : Results.NotFound(new { error = "tracking token not found" });
});

app.MapPost("/r/{trackingToken}/stop", async (
    string trackingToken,
    StopRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var ok = await repository.RecordStopAsync(trackingToken, request, cancellationToken);
    return ok ? Results.Ok(new { status = "stopped" }) : Results.NotFound(new { error = "tracking token not found" });
});

app.MapGet("/api/advisor-actions", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var actions = await repository.GetAdvisorActionsAsync(cancellationToken);
    return Results.Ok(actions);
});

app.MapPost("/api/advisor-actions/{actionId:guid}/assign", async (
    Guid actionId,
    AssignActionRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.AssignedTo))
    {
        return Results.BadRequest(new { error = "assignedTo is required" });
    }

    var ok = await repository.AssignAdvisorActionAsync(actionId, request, cancellationToken);
    return ok ? Results.Ok(new { status = "assigned", actionId, request.AssignedTo }) : Results.NotFound(new { error = "action not found" });
});

app.MapPost("/api/advisor-actions/{actionId:guid}/close", async (
    Guid actionId,
    CloseActionRequest request,
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Outcome))
    {
        return Results.BadRequest(new { error = "outcome is required" });
    }

    var ok = await repository.CloseAdvisorActionAsync(actionId, request, cancellationToken);
    return ok ? Results.Ok(new { status = "closed", actionId, request.Outcome }) : Results.NotFound(new { error = "action not found" });
});

app.MapGet("/api/reports/funnel", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var report = await repository.GetFunnelReportAsync(cancellationToken);
    return Results.Ok(report);
});

app.MapGet("/api/reports/blocked-reasons", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var report = await repository.GetBlockedReasonsAsync(cancellationToken);
    return Results.Ok(report);
});

app.MapGet("/api/reports/sla", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var report = await repository.GetSlaReportAsync(cancellationToken);
    return Results.Ok(report);
});

app.MapGet("/api/reports/opportunity-7day", async (
    PrototypeRepository repository,
    CancellationToken cancellationToken) =>
{
    var report = await repository.GetOpportunity7DayAsync(cancellationToken);
    return Results.Ok(report);
});

app.Run();
