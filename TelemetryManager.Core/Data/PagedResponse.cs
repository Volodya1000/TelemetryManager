namespace TelemetryManager.Core.Data;

public class PagedResponse<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }
    public List<T> Data { get; init; }

    public PagedResponse(List<T> data, int totalRecords, int pageNumber, int pageSize)
    {
        Data = data;
        TotalRecords = totalRecords;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((decimal)totalRecords / pageSize);
    }
}