using Application.Abstractions.Messaging;
using Domain.Comments;
using Domain.Contacts;
using SharedKernel;

namespace Application.Contacts.Delete;

internal sealed class DeleteContactCommandHandler(IContactRepository contactRepository)
    : ICommandHandler<DeleteContactCommand>
{
    public async Task<Result> Handle(DeleteContactCommand command, CancellationToken cancellationToken)
    {
        await contactRepository.DeleteAsync(command.Id);

        return Result.Success();
    }
}
