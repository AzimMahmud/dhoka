using Application.Abstractions.Messaging;
using Domain;

namespace Application.Posts.Search;

public sealed record SearchPostsQuery(PostSearchRequest SearchRequest) : IQuery<PagedSearchResult<PostsResponse>>;
