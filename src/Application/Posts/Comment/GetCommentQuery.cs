using Application.Abstractions.Messaging;
using Domain.Comments;

namespace Application.Posts.Comment;

public sealed record GetCommentQuery(Guid PostId) : IQuery<List<CommentsResponse>>;
