using Application.Abstractions.Messaging;

namespace Application.Posts.Search;

public sealed record SearchPostsQuery(string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<SearchPagedList>;
