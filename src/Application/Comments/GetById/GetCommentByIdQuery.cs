using Application.Abstractions.Messaging;
using Domain.Comments;

namespace Application.Comments.GetById;

public sealed record GetCommentByIdQuery(Guid CommentId) : IQuery<CommentResponse>;
