using Application.Abstractions.Messaging;
using Domain;
using Domain.Comments;

namespace Application.Comments.Get;

public sealed record GetCommentsQuery( Guid PostId, int PageSize, string? PaginationToken) : IQuery<PagedResult<CommentsResponse>>;
