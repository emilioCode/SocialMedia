using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Core.Entities;

namespace SocialMedia.Infrastructure.Data.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            var entity = builder;
            entity.ToTable("Usuario");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("IdUsuario");

            entity.Property(e => e.LastName)
                .HasColumnName("Apellidos")
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.DateBirth)
                .HasColumnName("FechaNacimiento")
                .HasColumnType("date");

            entity.Property(e => e.FirstName)
                .HasColumnName("Nombres")
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Telephone)
                .HasColumnName("Telefono")
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.IsActive)
                .HasColumnName("Activo");
        }
    }
}
