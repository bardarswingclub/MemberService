namespace MemberService.Data.Inventory;

public class InventoryBorrowItem
{
    public Guid Id { get; set; }
    public Guid BorrowId { get; set; }
    public InventoryBorrow Borrow { get; set; } = null!;
    public Guid AssetId { get; set; }
    public InventoryAsset Asset { get; set; } = null!;
    public DateTime ScannedAt { get; set; }
}
