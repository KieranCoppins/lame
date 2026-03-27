using Lame.DomainModel;

namespace Lame.TestingHelpers;

public class PaginatedResponseBuilder<T>
{
    private readonly PaginatedResponse<T> _response;

    public PaginatedResponseBuilder(List<T> items)
    {
        _response = new PaginatedResponse<T>
        {
            Page = 0,
            TotalPages = 1,
            Items = items
        };
    }

    public PaginatedResponse<T> Build()
    {
        return _response;
    }

    public PaginatedResponseBuilder<T> WithTotalPages(int totalPages)
    {
        _response.TotalPages = totalPages;
        return this;
    }
}