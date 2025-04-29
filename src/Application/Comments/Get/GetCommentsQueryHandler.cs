using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Posts.Get;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Get;

internal sealed class GetCommentsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCommentsQuery, PagedList<CommentsResponse>>
{
    public async Task<Result<PagedList<CommentsResponse>>> Handle(GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Comment> commentsQuery = context.Comments;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            commentsQuery = commentsQuery.Where(p =>
                p.PostId.ToString().Contains(request.SearchTerm));
        }

        if (request.SortOrder?.ToLower() == "desc")
        {
            commentsQuery = commentsQuery.OrderByDescending(GetSortProperty(request));
        }
        else
        {
            commentsQuery = commentsQuery.OrderBy(GetSortProperty(request));
        }

        IQueryable<CommentsResponse> productResponsesQuery = commentsQuery
            .Select(p => new CommentsResponse(
                p.Id,
                p.PostId,
                p.ContactInfo,
                p.Description));

        var posts = await PagedList<CommentsResponse>.CreateAsync(
            productResponsesQuery,
            request.Page,
            request.PageSize);

        return posts;
    }

    private static Expression<Func<Comment, object>> GetSortProperty(GetCommentsQuery request) =>
        request.SortColumn?.ToLower() switch
        {
            "postId" => product => product.PostId,
            _ => product => product.Id
        };
}
