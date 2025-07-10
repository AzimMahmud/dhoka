using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;
using Status = Domain.Posts.Status;

namespace Application.Posts.Init;

internal sealed class InitPostCommandHandler(IPostRepository postRepository) : ICommandHandler<InitPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(InitPostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post()
        {
            Id = Guid.NewGuid(),
            Status = nameof(Status.Init),
            CreatedAt = DateTime.UtcNow
        };
        
        await postRepository.CreateAsync(post);

        return post.Id;
        
    }
}
