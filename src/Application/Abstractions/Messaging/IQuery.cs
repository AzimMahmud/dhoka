using MediatR;
using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
public interface IPagedQuery<TResponse> : IRequest<Result<TResponse>>;
