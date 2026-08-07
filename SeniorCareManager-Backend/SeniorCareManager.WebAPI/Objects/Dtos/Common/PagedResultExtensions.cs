namespace SeniorCareManager.WebAPI.Objects.Dtos.Common;

public static class PagedResultExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int page, int pageSize)
    {
        var materialized = source as IReadOnlyCollection<T> ?? source.ToList();
        var items = materialized.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = materialized.Count,
        };
    }
}
