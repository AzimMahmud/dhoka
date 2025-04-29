﻿using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).HasMaxLength(200);
        builder.Property(u => u.LastName).HasMaxLength(200);
        builder.Property(u => u.Email).HasMaxLength(300);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.EmailVerified);

        builder.HasIndex(u => u.Email).IsUnique();
    }
}
