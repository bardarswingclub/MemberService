namespace MemberService.Data.Inventory;

public enum BorrowType
{
    Borrow,
    Return,
    InventoryCheck,
}

public class InventoryBorrow
{
    public Guid Id { get; set; }
    public string BorrowedByUserId { get; set; } = null!;
    public User BorrowedByUser { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public BorrowType Type { get; set; }

    // Navigation properties
    public ICollection<InventoryBorrowItem> Items { get; set; } = new List<InventoryBorrowItem>();
    public ICollection<InventoryAsset> HeldAssets { get; set; } = new List<InventoryAsset>();
}
