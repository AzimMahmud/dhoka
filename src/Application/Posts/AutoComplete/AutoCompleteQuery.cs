

using Application.Abstractions.Messaging;

namespace Application.Posts.AutoComplete;

public record AutoCompleteQuery(string? SearchTerm) : IQuery<List<AutoCompleteResponse>>;
