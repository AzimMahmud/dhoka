using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.GetImage;

internal sealed class GetImageQueryHandler(IApplicationDbContext context) : IQueryHandler<GetImageQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        List<string>? imageUrls = await context.Posts.Where(x => x.Id == request.PostId).AsNoTracking().Select(x => x.ImageUrls)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return Result.Success(imageUrls ?? new List<string>());
    }
}
