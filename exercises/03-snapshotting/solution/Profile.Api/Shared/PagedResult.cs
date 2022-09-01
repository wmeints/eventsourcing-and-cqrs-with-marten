namespace Profile.Api.Shared;

public record PagedResult<T>(IEnumerable<T> Items, int PageIndex, int PageSize, long TotalItemCount);