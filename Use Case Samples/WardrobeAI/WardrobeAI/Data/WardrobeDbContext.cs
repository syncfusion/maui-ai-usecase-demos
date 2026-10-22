using Microsoft.EntityFrameworkCore;
using WardrobeAI.Models;

namespace WardrobeAI.Data;

/// <summary>
/// EF Core SQLite context for the AI Wardrobe Assistant. Backs the
/// <see cref="User"/>, <see cref="ClothingItem"/>, <see cref="Outfit"/>,
/// <see cref="OutfitItem"/>, <see cref="PlannedOutfit"/> and
/// <see cref="ShoppingRecommendation"/> aggregates (SDS §8).
///
/// <para>Schema is intentionally hand-rolled (not <c>EnsureCreated</c>) so
/// EF Core's <c>dotnet ef migrations add Initial</c> output maps cleanly to
/// the explicit constraints here. The only column-level cap is on
/// <c>Users.StylePreference</c> (64 chars) — referenced by
/// <c>ProfileViewModel</c> when it joins the user's free-form style chips
/// into a single CSV string.</para>
/// </summary>
public sealed class WardrobeDbContext : DbContext
{
    public WardrobeDbContext(DbContextOptions<WardrobeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();
    public DbSet<Outfit> Outfits => Set<Outfit>();
    public DbSet<OutfitItem> OutfitItems => Set<OutfitItem>();
    public DbSet<PlannedOutfit> PlannedOutfits => Set<PlannedOutfit>();
    public DbSet<ShoppingRecommendation> ShoppingRecommendations => Set<ShoppingRecommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- User ----
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).ValueGeneratedOnAdd();
            entity.Property(u => u.Name).IsRequired().HasMaxLength(120);
            entity.Property(u => u.Email).HasMaxLength(254);
            entity.Property(u => u.StylePreference).HasMaxLength(64);
            entity.Property(u => u.Location).HasMaxLength(120);
            entity.Property(u => u.AgeGroup).HasMaxLength(32);
        });

        // ---- ClothingItem ----
        modelBuilder.Entity<ClothingItem>(entity =>
        {
            entity.ToTable("ClothingItems");
            entity.HasKey(c => c.ItemId);
            entity.Property(c => c.ItemId).ValueGeneratedOnAdd();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Category).HasMaxLength(64);
            entity.Property(c => c.Color).HasMaxLength(32);
            entity.Property(c => c.Pattern).HasMaxLength(32);
            entity.Property(c => c.Fabric).HasMaxLength(32);
            entity.Property(c => c.Brand).HasMaxLength(120);
            entity.Property(c => c.Season).HasMaxLength(32);
            entity.Property(c => c.Style).HasMaxLength(32);
            entity.Property(c => c.Occasion).HasMaxLength(32);

            entity.HasOne(c => c.User)
                  .WithMany(u => u.ClothingItems)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Outfit ----
        modelBuilder.Entity<Outfit>(entity =>
        {
            entity.ToTable("Outfits");
            entity.HasKey(o => o.OutfitId);
            entity.Property(o => o.OutfitId).ValueGeneratedOnAdd();
            entity.Property(o => o.Name).IsRequired().HasMaxLength(120);
            entity.Property(o => o.Occasion).HasMaxLength(64);

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Outfits)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- OutfitItem (join table) ----
        modelBuilder.Entity<OutfitItem>(entity =>
        {
            entity.ToTable("OutfitItems");
            entity.HasKey(oi => oi.OutfitItemId);
            entity.Property(oi => oi.OutfitItemId).ValueGeneratedOnAdd();

            entity.HasOne(oi => oi.Outfit)
                  .WithMany(o => o.OutfitItems)
                  .HasForeignKey(oi => oi.OutfitId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.ClothingItem)
                  .WithMany(c => c.OutfitItems)
                  .HasForeignKey(oi => oi.ClothingItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- PlannedOutfit ----
        modelBuilder.Entity<PlannedOutfit>(entity =>
        {
            entity.ToTable("PlannedOutfits");
            entity.HasKey(p => p.PlanId);
            entity.Property(p => p.PlanId).ValueGeneratedOnAdd();
            entity.Property(p => p.Name).HasMaxLength(120);
            entity.Property(p => p.Occasion).HasMaxLength(64);

            entity.HasOne(p => p.Outfit)
                  .WithMany(o => o.PlannedOutfits)
                  .HasForeignKey(p => p.OutfitId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- ShoppingRecommendation ----
        modelBuilder.Entity<ShoppingRecommendation>(entity =>
        {
            entity.ToTable("ShoppingRecommendations");
            entity.HasKey(s => s.RecommendationId);
            entity.Property(s => s.RecommendationId).ValueGeneratedOnAdd();
            entity.Property(s => s.ItemName).IsRequired().HasMaxLength(120);
            entity.Property(s => s.Reason).HasMaxLength(500);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.ShoppingRecommendations)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
