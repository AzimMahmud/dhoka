using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Get;

internal sealed class GetUsersQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetUsersQuery, PagedList<UsersResponse>>
{
    public async Task<Result<PagedList<UsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        
        IQueryable<User> productsQuery = context.Users;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Email.Contains(request.SearchTerm));
        }

        if (request.SortOrder?.ToLower() == "desc")
        {
            productsQuery = productsQuery.OrderByDescending(GetSortProperty(request));
        }
        else
        {
            productsQuery = productsQuery.OrderBy(GetSortProperty(request));
        }
        
        IQueryable<UsersResponse> productResponsesQuery = productsQuery
            .Select(p => new UsersResponse(
                p.Id,
                p.Email,
                p.FirstName,
                p.LastName));

        var users = await PagedList<UsersResponse>.CreateAsync(
            productResponsesQuery,
            request.Page,
            request.PageSize);

       
        
        return users;
    }
    
    private static Expression<Func<User, object>> GetSortProperty(GetUsersQuery request) =>
        request.SortColumn?.ToLower() switch
        {
            "email" => product => product.Email,
            _ => product => product.Id
        };
}
