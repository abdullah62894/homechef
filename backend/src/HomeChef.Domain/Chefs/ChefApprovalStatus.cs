namespace HomeChef.Domain.Chefs;

/// <summary>
/// Approval status for chef profiles. New chefs start as PendingApproval.
/// Only Approved chefs appear publicly.
/// </summary>
public enum ChefApprovalStatus
{
    PendingApproval = 0,
    Approved = 1,
    Rejected = 2,
}
