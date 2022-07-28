using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialMedia.Core.Entities;

namespace SocialMedia.Infrastructure.Data.Configurations
{
    internal class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            var entity = builder;
            entity.ToTable("Comentario"); 

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("IdComentario") 
                .ValueGeneratedNever();

            entity.Property(e => e.PostId)
                .HasColumnName("IdPublicacion");

            entity.Property(e => e.UserId)
                .HasColumnName("IdUsuario");

            entity.Property(e => e.Description)
                .HasColumnName("Descripcion")
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.Property(e => e.Date)
                .HasColumnName("Fecha") 
                .HasColumnType("datetime");

            entity.Property(e => e.IsActive)
                .HasColumnName("Activo"); 

            entity.HasOne(d => d.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comentario_Publicacion");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comentario_Usuario");
        }
    }
}
