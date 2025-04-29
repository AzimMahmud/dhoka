using Domain.SearchEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.SearchEvents;

public class SearchEventConfiguration : IEntityTypeConfiguration<SearchEvent>
{
    public void Configure(EntityTypeBuilder<SearchEvent> builder)
    {
        builder.HasKey(t => t.Id);
    }
}
