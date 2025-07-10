using Application.Abstractions.Messaging;

namespace Application.Posts.Admin.GetById;

public sealed record GetPostByIdQuery(Guid PostId) : IQuery<PostResponse>;
