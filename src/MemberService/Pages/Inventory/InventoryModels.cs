namespace MemberService.Pages.Inventory;

public class InventoryAssetDto
{
    public Guid Id { get; set; }
    public required string Tag { get; set; }
    public string? Kategori { get; set; }
    public string? SubKategori { get; set; }
    public required string Beskrivelse { get; set; }
    public string? Merke { get; set; }
    public string? Modell { get; set; }
    public string? Detaljer { get; set; }
    public decimal? LengdeM { get; set; }
    public int? Diameter { get; set; }
    public string? PhotoUrl { get; set; }
    public bool InInventory { get; set; }
    public string? Lokasjon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CurrentBorrowId { get; set; }
    public string? BorrowedByEventName { get; set; }
    public string? BorrowedByUserName { get; set; }
}

public class CreateAssetRequest
{
    public required string Tag { get; set; }
    public string? Kategori { get; set; }
    public string? SubKategori { get; set; }
    public required string Beskrivelse { get; set; }
    public string? Merke { get; set; }
    public string? Modell { get; set; }
    public string? Detaljer { get; set; }
    public decimal? LengdeM { get; set; }
    public int? Diameter { get; set; }
    public string? PhotoUrl { get; set; }
    public bool InInventory { get; set; } = true;
    public string? Lokasjon { get; set; }
}

public class UpdateAssetRequest
{
    public string? Kategori { get; set; }
    public string? SubKategori { get; set; }
    public required string Beskrivelse { get; set; }
    public string? Merke { get; set; }
    public string? Modell { get; set; }
    public string? Detaljer { get; set; }
    public decimal? LengdeM { get; set; }
    public int? Diameter { get; set; }
    public string? PhotoUrl { get; set; }
    public bool InInventory { get; set; }
    public string? Lokasjon { get; set; }
}

public class BorrowSessionDto
{
    public Guid Id { get; set; }
    public string BorrowedByUserId { get; set; }
    public string BorrowedByUserName { get; set; }
    public required string EventName { get; set; }
    public required string Type { get; set; } // "Borrow", "Return", "InventoryCheck"
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<BorrowItemDto> Items { get; set; } = [];
}

public class BorrowItemDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Tag { get; set; }
    public string Beskrivelse { get; set; }
    public DateTime ScannedAt { get; set; }
}

public class StartBorrowRequest
{
    public required string EventName { get; set; }
    public required string Type { get; set; } // "Borrow", "Return", "InventoryCheck"
}

public class ScanAssetRequest
{
    public required string Tag { get; set; }
}

public class CsvImportResult
{
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<CsvImportError> Errors { get; set; } = [];
}

public class CsvImportError
{
    public int RowNumber { get; set; }
    public required string Message { get; set; }
}

public class CsvImportRequest
{
    public required string Content { get; set; }
}

public class AssetLookupRequest
{
    public required List<string> Tags { get; set; }
}

public class AssetLookupResult
{
    public List<InventoryAssetDto> Assets { get; set; } = [];
    public List<string> NotFound { get; set; } = [];
}
