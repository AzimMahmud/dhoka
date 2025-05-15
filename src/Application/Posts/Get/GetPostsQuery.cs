using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;

namespace Application.Posts.Get;

public sealed record GetPostsQuery(PostSearchRequest SearchRequest) : IQuery<PagedResult<PostsResponse>>;
