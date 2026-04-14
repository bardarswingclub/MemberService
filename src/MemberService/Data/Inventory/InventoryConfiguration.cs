using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MemberService.Data.Inventory;

public class InventoryAssetConfiguration : IEntityTypeConfiguration<InventoryAsset>
{
    public void Configure(EntityTypeBuilder<InventoryAsset> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Tag)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Tag)
            .IsUnique();

        builder.Property(e => e.Kategori)
            .HasMaxLength(100);

        builder.Property(e => e.SubKategori)
            .HasMaxLength(100);

        builder.Property(e => e.Beskrivelse)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Merke)
            .HasMaxLength(100);

        builder.Property(e => e.Modell)
            .HasMaxLength(100);

        builder.Property(e => e.Detaljer)
            .HasMaxLength(500);

        builder.Property(e => e.LengdeM)
            .HasPrecision(10, 2); // Decimal with max 10 digits, 2 decimal places

        builder.Property(e => e.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Lokasjon)
            .HasMaxLength(200);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Foreign key to InventoryBorrow
        builder.HasOne(e => e.CurrentBorrow)
            .WithMany(b => b.HeldAssets)
            .HasForeignKey(e => e.CurrentBorrowId)
            .OnDelete(DeleteBehavior.SetNull);

        // Navigation: BorrowItems
        builder.HasMany(e => e.BorrowItems)
            .WithOne(bi => bi.Asset)
            .HasForeignKey(bi => bi.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InventoryBorrowConfiguration : IEntityTypeConfiguration<InventoryBorrow>
{
    public void Configure(EntityTypeBuilder<InventoryBorrow> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BorrowedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.EventName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.StartedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Foreign key to User
        builder.HasOne(e => e.BorrowedByUser)
            .WithMany()
            .HasForeignKey(e => e.BorrowedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation: Items
        builder.HasMany(e => e.Items)
            .WithOne(bi => bi.Borrow)
            .HasForeignKey(bi => bi.BorrowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: HeldAssets
        builder.HasMany(e => e.HeldAssets)
            .WithOne(a => a.CurrentBorrow)
            .HasForeignKey(a => a.CurrentBorrowId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class InventoryBorrowItemConfiguration : IEntityTypeConfiguration<InventoryBorrowItem>
{
    public void Configure(EntityTypeBuilder<InventoryBorrowItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BorrowId)
            .IsRequired();

        builder.Property(e => e.AssetId)
            .IsRequired();

        builder.Property(e => e.ScannedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Unique constraint: can't scan the same asset twice into the same borrow session
        builder.HasIndex(e => new { e.BorrowId, e.AssetId })
            .IsUnique();

        // Foreign keys
        builder.HasOne(e => e.Borrow)
            .WithMany(b => b.Items)
            .HasForeignKey(e => e.BorrowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Asset)
            .WithMany(a => a.BorrowItems)
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
