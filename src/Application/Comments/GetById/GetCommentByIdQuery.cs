using Application.Abstractions.Messaging;

namespace Application.Comments.GetById;

public sealed record GetCommentByIdQuery(Guid CommentId) : IQuery<CommentResponse>;
