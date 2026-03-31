namespace MemberService.Pages.Inventory;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.Inventory;

[ApiController]
[Route("api/inventory/assets")]
[Authorize]
public class InventoryAssetsController(MemberContext context, CsvImportService csvImportService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CanBorrowInventory")]
    public async Task<ActionResult<IEnumerable<InventoryAssetDto>>> GetAssets([FromQuery] string? search, [FromQuery] bool borrowedOnly = false)
    {
        var query = context.InventoryAssets
            .Include(a => a.CurrentBorrow)
            .ThenInclude(b => b.BorrowedByUser)
            .AsQueryable();

        if (borrowedOnly)
        {
            query = query.Where(a => a.CurrentBorrowId != null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(a =>
                a.Tag.ToLower().Contains(searchLower) ||
                a.Beskrivelse.ToLower().Contains(searchLower) ||
                (a.Merke != null && a.Merke.ToLower().Contains(searchLower)) ||
                (a.Modell != null && a.Modell.ToLower().Contains(searchLower))
            );
        }

        var assets = await query.OrderBy(a => a.Tag).ToListAsync();

        return Ok(assets.Select(MapToDto));
    }

    [HttpGet("{tag}")]
    [Authorize(Policy = "CanBorrowInventory")]
    public async Task<ActionResult<InventoryAssetDto>> GetAsset(string tag)
    {
        var asset = await context.InventoryAssets
            .Include(a => a.CurrentBorrow)
            .ThenInclude(b => b.BorrowedByUser)
            .FirstOrDefaultAsync(a => a.Tag == tag);

        if (asset == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(asset));
    }

    [HttpPost]
    [Authorize(Policy = "CanManageInventory")]
    public async Task<ActionResult<InventoryAssetDto>> CreateAsset([FromBody] CreateAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Tag) || string.IsNullOrWhiteSpace(request.Beskrivelse))
        {
            return BadRequest("Tag and Beskrivelse are required");
        }

        var existing = await context.InventoryAssets.FirstOrDefaultAsync(a => a.Tag == request.Tag);
        if (existing != null)
        {
            return Conflict($"Asset with tag '{request.Tag}' already exists");
        }

        var asset = new InventoryAsset
        {
            Id = Guid.NewGuid(),
            Tag = request.Tag,
            Kategori = request.Kategori,
            SubKategori = request.SubKategori,
            Beskrivelse = request.Beskrivelse,
            Merke = request.Merke,
            Modell = request.Modell,
            Detaljer = request.Detaljer,
            LengdeM = request.LengdeM,
            Diameter = request.Diameter,
            PhotoUrl = request.PhotoUrl,
            InInventory = request.InInventory,
            Lokasjon = request.Lokasjon,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.InventoryAssets.Add(asset);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAsset), new { tag = asset.Tag }, MapToDto(asset));
    }

    [HttpPut("{tag}")]
    [Authorize(Policy = "CanManageInventory")]
    public async Task<ActionResult<InventoryAssetDto>> UpdateAsset(string tag, [FromBody] UpdateAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Beskrivelse))
        {
            return BadRequest("Beskrivelse is required");
        }

        var asset = await context.InventoryAssets.FirstOrDefaultAsync(a => a.Tag == tag);
        if (asset == null)
        {
            return NotFound();
        }

        asset.Kategori = request.Kategori;
        asset.SubKategori = request.SubKategori;
        asset.Beskrivelse = request.Beskrivelse;
        asset.Merke = request.Merke;
        asset.Modell = request.Modell;
        asset.Detaljer = request.Detaljer;
        asset.LengdeM = request.LengdeM;
        asset.Diameter = request.Diameter;
        asset.PhotoUrl = request.PhotoUrl;
        asset.InInventory = request.InInventory;
        asset.Lokasjon = request.Lokasjon;
        asset.UpdatedAt = DateTime.UtcNow;

        context.InventoryAssets.Update(asset);
        await context.SaveChangesAsync();

        return Ok(MapToDto(asset));
    }

    [HttpDelete("{tag}")]
    [Authorize(Policy = "CanManageInventory")]
    public async Task<IActionResult> DeleteAsset(string tag)
    {
        var asset = await context.InventoryAssets.FirstOrDefaultAsync(a => a.Tag == tag);
        if (asset == null)
        {
            return NotFound();
        }

        context.InventoryAssets.Remove(asset);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("import")]
    [Authorize(Policy = "CanManageInventory")]
    public async Task<ActionResult<CsvImportResult>> ImportCsv([FromBody] CsvImportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("CSV content is required");
        }

        var result = await csvImportService.ImportAsync(request.Content);
        return Ok(result);
    }

    private static InventoryAssetDto MapToDto(InventoryAsset asset)
    {
        return new InventoryAssetDto
        {
            Id = asset.Id,
            Tag = asset.Tag,
            Kategori = asset.Kategori,
            SubKategori = asset.SubKategori,
            Beskrivelse = asset.Beskrivelse,
            Merke = asset.Merke,
            Modell = asset.Modell,
            Detaljer = asset.Detaljer,
            LengdeM = asset.LengdeM,
            Diameter = asset.Diameter,
            PhotoUrl = asset.PhotoUrl,
            InInventory = asset.InInventory,
            Lokasjon = asset.Lokasjon,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt,
            CurrentBorrowId = asset.CurrentBorrowId,
            BorrowedByEventName = asset.CurrentBorrow?.EventName,
            BorrowedByUserName = asset.CurrentBorrow?.BorrowedByUser?.FullName
                ?? asset.CurrentBorrow?.BorrowedByUser?.UserName,
        };
    }
}
