using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.AutoComplete;

public class AutoCompleteQueryHandler(IPostRepository postRepository, IDateTimeProvider dateTimeProvider)     : IQueryHandler<AutoCompleteQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(AutoCompleteQuery request, CancellationToken cancellationToken)
    {
        List<string> response = await postRepository.AutocompleteTitlesAsync(request.SearchRequest);

        return response;
    }
}
