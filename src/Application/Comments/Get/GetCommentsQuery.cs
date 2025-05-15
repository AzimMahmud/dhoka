using Application.Abstractions.Messaging;
using Application.Posts.Get;
using Domain;
using Domain.Comments;

namespace Application.Comments.Get;

public sealed record GetCommentsQuery( Guid PostId,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize, string PaginationToken) : IQuery<PagedResult<CommentsResponse>>;
