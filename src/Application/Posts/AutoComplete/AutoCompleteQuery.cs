

using Application.Abstractions.Messaging;
using Domain.Posts;

namespace Application.Posts.AutoComplete;

public record AutoCompleteQuery(AutocompleteRequest SearchRequest) : IQuery<List<string>>;
