using Application.Abstractions.Messaging;

namespace Application.Posts.GetImage;

public sealed record GetImageQuery(Guid PostId) : IQuery<List<string>>;
