using Application.Posts.Get;
using MediatR;
using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

public interface IPagedQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<PagedList<TResponse>>>
    where TQuery : IPagedQuery<PagedList<TResponse>>;


