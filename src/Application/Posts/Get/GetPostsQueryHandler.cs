using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Get;

internal sealed class GetPostsQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostsQuery, PagedResult<PostsResponse>>
{
    public async Task<Result<PagedResult<PostsResponse>>> Handle(GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        
        PagedResult<PostsResponse> posts = await postRepository.SearchAsync(request.SearchRequest);

        return posts;

        // IQueryable<Post> productsQuery = context.Posts;
        //
        // if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        // {
        //     productsQuery = productsQuery.Where(p =>
        //         p.Title.Contains(request.SearchTerm) ||
        //         p.PaymentType.Contains(request.SearchTerm) ||
        //         p.TransactionMode.Contains(request.SearchTerm) ||
        //         p.MobilNumbers.Contains(request.SearchTerm));
        // }
        //
        // if (request.SortOrder?.ToLower() == "desc")
        // {
        //     productsQuery = productsQuery.OrderByDescending(GetSortProperty(request));
        // }
        // else
        // {
        //     productsQuery = productsQuery.OrderBy(GetSortProperty(request));
        // }
        //
        // productsQuery = string.IsNullOrEmpty(request.Status)
        //     ? productsQuery.Where(x => x.Status.ToLower() == nameof(Status.Approved).ToLower())
        //     : productsQuery.Where(x => x.Status.ToLower() ==  request.Status.ToLower());
        //
        //
        // productsQuery = productsQuery.Where(x => x.Status != nameof(Status.Init));
        // IQueryable<PostsResponse> productResponsesQuery = productsQuery
        //     .AsNoTracking()
        //     .Select(p => new PostsResponse(
        //         p.Id,
        //         p.Title,
        //         p.TransactionMode,
        //         p.PaymentType,
        //         p.Description,
        //         p.MobilNumbers,
        //         p.Amount,
        //         p.Status,
        //         p.CreatedAt));
        //
        // var posts = await PagedList<PostsResponse>.CreateAsync(
        //     productResponsesQuery,
        //     request.Page,
        //     request.PageSize);
        //
        // return posts;
    }

    // private static Expression<Func<Post, object>> GetSortProperty(GetPostsQuery request) =>
    //     request.SortColumn?.ToLower() switch
    //     {
    //         "title" => product => product.Title,
    //         "transactionMode" => product => product.TransactionMode,
    //         "paymentType" => product => product.PaymentType,
    //         _ => product => product.Id
    //     };
}
