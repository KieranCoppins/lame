namespace Lame.DomainModel;

public class PaginatedResponse<T>
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; }
}