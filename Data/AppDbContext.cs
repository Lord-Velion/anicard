using AniCard.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AniCard.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Character> Characters { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Character>(entity =>
            {
                entity.ToTable("Character");
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Tags)
                      .WithMany(t => t.Characters)
                      .UsingEntity<Dictionary<string, object>>(
                          "CharacterTag",
                          j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Restrict),
                          j => j.HasOne<Character>().WithMany().HasForeignKey("CharacterId").OnDelete(DeleteBehavior.Cascade),
                          j =>
                          {
                              j.HasKey("CharacterId", "TagId");
                              j.ToTable("CharacterTag");
                          });
            });

            builder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tag");
                entity.HasIndex(t => t.Name).IsUnique();
            });
        }
    }
}
