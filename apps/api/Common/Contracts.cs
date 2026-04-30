namespace AutoVhc.Api.Common;

public sealed record ImportFollowUpRequest(
    string SiteCode,
    DateOnly? DueFrom,
    DateOnly? DueTo,
    int RecordLimit = 100,
    int Offset = 0,
    string Order = "due_date asc",
    int LanguageId = 1);

public sealed record CreateRemindersRequest(List<string> FollowUpItemIds);
public sealed record SendMockRequest(List<Guid>? MessageIds);
public sealed record CallbackRequest(string PreferredTime, string PhoneNumber, string? Note);
public sealed record RemindLaterRequest(int Days = 7, string? Note = null);
public sealed record AlreadyRepairedRequest(string? Note);
public sealed record StopRequest(string? Channel, string? Reason);
public sealed record AssignActionRequest(string AssignedTo);
public sealed record CloseActionRequest(string Outcome, string? Notes, bool StopFurtherReminders = false);

public sealed record MockFollowUpRecord(
    string FollowUpItemId,
    string EventType,
    string SiteCode,
    string CustomerId,
    string CustomerName,
    string? MobileNumber,
    string? EmailAddress,
    string? PreferredChannel,
    string VehicleRegistration,
    string VehicleMakeModel,
    string FollowUpDescription,
    DateOnly OriginalInspectionDate,
    DateOnly DueDate,
    string ItemStatus,
    bool DisableFollowUp,
    decimal EstimatedValue,
    string LanguageCode,
    int LanguageId);
