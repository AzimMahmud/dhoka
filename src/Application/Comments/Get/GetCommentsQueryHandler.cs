using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Posts.Get;
using Domain;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Get;

internal sealed class GetCommentsQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentsQuery, PagedResult<CommentsResponse>>
{
    public async Task<Result<PagedResult<CommentsResponse>>> Handle(GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<CommentsResponse> comments =
            await commentRepository.GetByPostIdPaginatedAsync(request.PostId, request.PageSize,
                request.PaginationToken);


        //     IQueryable<Comment> commentsQuery = context.Comments;
        //
        //     if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //     {
        //         commentsQuery = commentsQuery.Where(p =>
        //             p.PostId.ToString().Contains(request.SearchTerm));
        //     }
        //
        //     if (request.SortOrder?.ToLower() == "desc")
        //     {
        //         commentsQuery = commentsQuery.OrderByDescending(GetSortProperty(request));
        //     }
        //     else
        //     {
        //         commentsQuery = commentsQuery.OrderBy(GetSortProperty(request));
        //     }
        //
        //     IQueryable<CommentsResponse> productResponsesQuery = commentsQuery
        //         .Select(p => new CommentsResponse
        //         {
        //             Id = p.Id,
        //             PostId = p.PostId,
        //             ContactInfo = p.ContactInfo,
        //             Description = p.Description,
        //             Created = p.CreatedAt
        //         });
        //
        //     var posts = await PagedList<CommentsResponse>.CreateAsync(
        //         productResponsesQuery,
        //         request.Page,
        //         request.PageSize);
        //
        //     return posts;

        return new Result<PagedResult<CommentsResponse>>(comments , true, null);
    }

    //
    // private static Expression<Func<Comment, object>> GetSortProperty(GetCommentsQuery request) =>
    //     request.SortColumn?.ToLower() switch
    //     {
    //         "postId" => product => product.PostId,
    //         _ => product => product.Id
    //     };
}
