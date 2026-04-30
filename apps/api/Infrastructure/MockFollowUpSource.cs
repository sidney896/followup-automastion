using AutoVhc.Api.Common;

namespace AutoVhc.Api.Infrastructure;

public sealed class MockFollowUpSource
{
    public IReadOnlyList<MockFollowUpRecord> GetRecords(string siteCode)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var records = new List<MockFollowUpRecord>
        {
            new(
                "FU-1001",
                "deferred_repair",
                "RIVER",
                "CUST-1001",
                "Sarah Bell",
                "+447700900111",
                "sarah@example.com",
                "sms",
                "AB12 CDE",
                "Ford Focus",
                "Front tyres approaching replacement",
                today.AddDays(-21),
                today.AddDays(-1),
                "due",
                false,
                280m,
                "en-GB",
                1),
            new(
                "FU-1002",
                "mot_reminder",
                "RIVER",
                "CUST-1002",
                "Tom Ridley",
                "+447700900222",
                "tom@example.com",
                "sms",
                "GK77 MOT",
                "Toyota Yaris",
                "MOT due in 28 days",
                today.AddDays(-120),
                today,
                "open",
                false,
                65m,
                "en-GB",
                1),
            new(
                "FU-1003",
                "deferred_repair",
                "RIVER",
                "CUST-1003",
                "Mina Clarke",
                null,
                "mina@example.com",
                "sms",
                "RX12 AAA",
                "BMW 1 Series",
                "Rear brake pads worn",
                today.AddDays(-10),
                today.AddDays(-2),
                "due",
                false,
                220m,
                "en-GB",
                1),
            new(
                "FU-1004",
                "service_reminder",
                "NORTH",
                "CUST-2001",
                "Chris Wong",
                "+447700900333",
                "chris@example.com",
                "email",
                "MN66 SER",
                "Audi A4",
                "Annual service due",
                today.AddDays(-300),
                today.AddDays(5),
                "open",
                false,
                340m,
                "en-GB",
                1),
            new(
                "FU-1005",
                "deferred_repair",
                "RIVER",
                "CUST-1005",
                "Alain Marchand",
                "+447700900444",
                "alain@example.com",
                "sms",
                "FR44 CAR",
                "Peugeot 208",
                "Battery advisory",
                today.AddDays(-15),
                today.AddDays(-3),
                "due",
                true,
                145m,
                "fr-FR",
                2)
        };

        return records.Where(r => string.Equals(r.SiteCode, siteCode, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
