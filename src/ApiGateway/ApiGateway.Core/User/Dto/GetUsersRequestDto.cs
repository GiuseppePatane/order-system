namespace ApiGateway.Core.User.Dto;

public record GetUsersRequestDto
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Validates the DTO and returns a list of validation error messages (empty if valid)
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (PageNumber < 1)
            errors.Add("PageNumber must be greater than or equal to 1");

        if (PageSize < 1 || PageSize > 100)
            errors.Add("PageSize must be between 1 and 100");

        return errors;
    }

    public Dictionary<string, string> ToQueryDictionary()
    {
        var dict = new Dictionary<string, string>
        {
            ["pageNumber"] = PageNumber.ToString(),
            ["pageSize"] = PageSize.ToString()
        };

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            dict["searchTerm"] = SearchTerm!;

        return dict;
    }
}
