using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Posts;

// internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
// {
//     public void Configure(EntityTypeBuilder<Post> builder)
//     {
//         builder.HasKey(t => t.Id);
//
//         builder.Property(t => t.SearchVector)
//             .HasColumnType("tsvector")
//             .HasComputedColumnSql(@"
//                to_tsvector('simple',
//                  coalesce(title,'')       || ' ' ||
//                  coalesce(description,'') || ' ' ||
//                  coalesce(transaction_mode,'') || ' ' ||
//                  coalesce(payment_type,'')     || ' ' ||
//                  array_to_string(mobil_numbers, ' ')
//                )", stored: true);
//         
//         
//         // GIN index on it:
//         builder
//             .HasIndex(t => t.SearchVector)
//             .HasMethod("GIN");
//
//         // Optional: trigram index on mobiles
//         builder
//             .HasIndex("MobilNumbers")
//             .HasMethod("GIN")
//             .HasOperators(new[] { "gin_trgm_ops" });
//     }
// }
