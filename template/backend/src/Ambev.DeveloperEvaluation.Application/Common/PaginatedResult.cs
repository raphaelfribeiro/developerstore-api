namespace Ambev.DeveloperEvaluation.Application.Common;

/// <summary>
/// Represents a paginated result set returned by list query handlers.
/// Decoupled from the WebApi layer's PaginatedList to preserve clean architecture.
/// </summary>
/// <typeparam name="T">The type of data items.</typeparam>
public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public PaginatedResult()
    {
    }

    public PaginatedResult(IEnumerable<T> data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
