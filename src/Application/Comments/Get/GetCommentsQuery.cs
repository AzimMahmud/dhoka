using Application.Abstractions.Messaging;
using Application.Posts.Get;

namespace Application.Comments.Get;

public sealed record GetCommentsQuery(string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<PagedList<CommentsResponse>>;
