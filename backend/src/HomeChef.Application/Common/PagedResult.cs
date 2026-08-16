namespace HomeChef.Application.Common;

/// <summary>Paginated collection used by list endpoints.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total,
    bool HasMore);