

using Application.Abstractions.Messaging;
using Domain;

namespace Application.Posts.AutoComplete;

public record AutoCompleteQuery(AutocompleteRequest SearchRequest) : IQuery<List<string>>;
