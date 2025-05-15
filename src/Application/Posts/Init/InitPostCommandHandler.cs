using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Init;

internal sealed class InitPostCommandHandler(IPostRepository postRepository, IDateTimeProvider dateTimeProvider) : ICommandHandler<InitPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(InitPostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            Status = nameof(Status.Init),
            CreatedAt = dateTimeProvider.UtcNow
        };
        
        await postRepository.CreateAsync(post);

        return post.Id;
        
    }
}
