using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.AutoComplete;

public class AutoCompleteQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)     : IQueryHandler<AutoCompleteQuery, List<AutoCompleteResponse>>
{
    public async Task<Result<List<AutoCompleteResponse>>> Handle(AutoCompleteQuery request, CancellationToken cancellationToken)
    {
        string searchTerm = request.SearchTerm;

        if (string.IsNullOrEmpty(searchTerm))
        {
            return Result.Success(new List<AutoCompleteResponse>());
        }
        
        List<AutoCompleteResponse> postsQuery = await context.Posts
            .FromSqlInterpolated($@"
                SELECT title, transaction_mode, payment_type, description
                FROM posts
                WHERE search_vector @@plainto_tsquery('simple', {searchTerm})
                ORDER BY ts_rank(search_vector, plainto_tsquery('simple', {searchTerm})) DESC
                LIMIT 5")
            .Select(p => new AutoCompleteResponse
            {
                Title = p.Title,
                TransactionMode = p.TransactionMode,
                PaymentType = p.PaymentType,
                Description = p.Description,
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);
        return postsQuery;
    }
}
