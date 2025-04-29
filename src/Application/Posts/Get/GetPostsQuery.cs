using Application.Abstractions.Messaging;

namespace Application.Posts.Get;

public sealed record GetPostsQuery(string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<PagedList<PostsResponse>>;
