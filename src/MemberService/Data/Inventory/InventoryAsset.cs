namespace MemberService.Data.Inventory;

public class InventoryAsset
{
    public Guid Id { get; set; }
    public string Tag { get; set; } = null!; // Unique key
    public string? Kategori { get; set; }
    public string? SubKategori { get; set; }
    public string Beskrivelse { get; set; } = null!;
    public string? Merke { get; set; }
    public string? Modell { get; set; }
    public string? Detaljer { get; set; }
    public decimal? LengdeM { get; set; }
    public int? Diameter { get; set; }
    public string? PhotoUrl { get; set; }
    public bool InInventory { get; set; } = true;
    public string? Lokasjon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastObservedAt { get; set; }

    // Denormalized: which borrow session currently holds this asset (null = available)
    public Guid? CurrentBorrowId { get; set; }
    public InventoryBorrow? CurrentBorrow { get; set; }

    // Navigation properties
    public ICollection<InventoryBorrowItem> BorrowItems { get; set; } = new List<InventoryBorrowItem>();
}
