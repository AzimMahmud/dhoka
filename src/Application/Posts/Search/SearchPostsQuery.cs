using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;

namespace Application.Posts.Search;

public sealed record SearchPostsQuery(PostSearchRequest SearchRequest) : IQuery<PagedResult<PostsResponse>>;
