using Application.Abstractions.Messaging;
using Domain;

namespace Application.Posts.Admin.Get;

public sealed record GetPostsQuery(int PageSize, string? PaginationToken, string status) : IQuery<PagedResult<PostsResponse>>;
