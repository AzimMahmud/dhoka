using Application.Abstractions.Messaging;

namespace Application.Posts.Get;

public sealed record GetPostsQuery(string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    string Status,
    int Page,
    int PageSize) : IQuery<PagedList<PostsResponse>>;
