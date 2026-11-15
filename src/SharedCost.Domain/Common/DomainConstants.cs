namespace SharedCost.Domain.Common;

public static class RoleCodes
{
    public const string Admin = "ADMIN";
    public const string Finance = "FINANCE";
    public const string Reviewer = "REVIEWER";
    public const string Viewer = "VIEWER";
    public const string Auditor = "AUDITOR";

    public static readonly string[] All = { Admin, Finance, Reviewer, Viewer, Auditor };
}

public static class CommonStatus
{
    public const string Draft = "DRAFT";
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Locked = "LOCKED";
    public const string Expired = "EXPIRED";
}

public static class SimulationStatus
{
    public const string Running = "RUNNING";
    public const string Completed = "COMPLETED";
    public const string Failed = "FAILED";
}

public static class AllocationRunStatus
{
    public const string Draft = "DRAFT";
    public const string Calculated = "CALCULATED";
    public const string PendingReview = "PENDING_REVIEW";
    public const string Reviewed = "REVIEWED";
    public const string Rejected = "REJECTED";
    public const string Locked = "LOCKED";
}

public static class ReviewDecisionStatus
{
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
}

public static class CalculationRules
{
    public const int RatioDecimalPlaces = 6;
    public const int AmountDecimalPlaces = 2;
    public const int DriverDecimalPlaces = 4;
    public const decimal ExpectedTotalRatio = 1.000000m;
}
