using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.AutoComplete;

public class AutoCompleteQueryHandler(IPostRepository postRepository, IDateTimeProvider dateTimeProvider)     : IQueryHandler<AutoCompleteQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(AutoCompleteQuery request, CancellationToken cancellationToken)
    {
        
        List<string> response = await postRepository.AutocompleteTitlesAsync(request.SearchRequest);

        return response;

        // string searchTerm = request.SearchTerm;
        //
        // if (string.IsNullOrEmpty(searchTerm))
        // {
        //     return Result.Success(new List<AutoCompleteResponse>());
        // }
        //
        // List<AutoCompleteResponse> postsQuery = await context.Posts
        //     .FromSqlInterpolated($@"
        //         SELECT title, transaction_mode, payment_type, description
        //         FROM posts
        //         WHERE search_vector @@plainto_tsquery('simple', {searchTerm})
        //         ORDER BY ts_rank(search_vector, plainto_tsquery('simple', {searchTerm})) DESC
        //         LIMIT 5")
        //     .Select(p => new AutoCompleteResponse
        //     {
        //         Title = p.Title,
        //         TransactionMode = p.TransactionMode,
        //         PaymentType = p.PaymentType,
        //         Description = p.Description,
        //     })
        //     .AsNoTracking()
        //     .ToListAsync(cancellationToken: cancellationToken);
        // return postsQuery;
    }
}
