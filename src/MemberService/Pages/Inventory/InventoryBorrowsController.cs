namespace MemberService.Pages.Inventory;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.Inventory;

[ApiController]
[Route("api/inventory/borrows")]
[Authorize(Policy = "CanBorrowInventory")]
public class InventoryBorrowsController(MemberContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BorrowSessionDto>>> GetSessions()
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

        var sessions = await context.InventoryBorrows
            .Include(b => b.Items)
            .Include(b => b.BorrowedByUser)
            .Where(b => b.BorrowedByUserId == userId)
            .OrderByDescending(b => b.StartedAt)
            .ToListAsync();

        return Ok(sessions.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BorrowSessionDto>> GetSession(Guid id)
    {
        var session = await context.InventoryBorrows
            .Include(b => b.Items)
            .ThenInclude(i => i.Asset)
            .Include(b => b.BorrowedByUser)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (session == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(session));
    }

    [HttpPost]
    public async Task<ActionResult<BorrowSessionDto>> StartSession([FromBody] StartBorrowRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName) || string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("EventName and Type are required");
        }

        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var borrowType = request.Type switch
        {
            "Borrow" => Data.Inventory.BorrowType.Borrow,
            "Return" => Data.Inventory.BorrowType.Return,
            "InventoryCheck" => Data.Inventory.BorrowType.InventoryCheck,
            _ => Data.Inventory.BorrowType.Borrow
        };

        var session = new InventoryBorrow
        {
            Id = Guid.NewGuid(),
            BorrowedByUserId = userId,
            EventName = request.EventName,
            Type = borrowType,
            StartedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        context.InventoryBorrows.Add(session);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, MapToDto(session));
    }

    [HttpPost("{id}/scan")]
    public async Task<ActionResult<BorrowSessionDto>> ScanAsset(Guid id, [FromBody] ScanAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Tag))
        {
            return BadRequest("Tag is required");
        }

        var session = await context.InventoryBorrows
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (session == null)
        {
            return NotFound();
        }

        if (session.CompletedAt.HasValue)
        {
            return BadRequest("Cannot scan into a completed session");
        }

        var asset = await context.InventoryAssets.FirstOrDefaultAsync(a => a.Tag == request.Tag);
        if (asset == null)
        {
            return NotFound($"Asset with tag '{request.Tag}' not found");
        }

        // Check for duplicate scan in this session (no-op if already scanned)
        if (!session.Items.Any(i => i.AssetId == asset.Id))
        {
            var borrowItem = new InventoryBorrowItem
            {
                Id = Guid.NewGuid(),
                BorrowId = session.Id,
                AssetId = asset.Id,
                ScannedAt = DateTime.UtcNow
            };

            context.InventoryBorrowItems.Add(borrowItem);
        }

        // Always update LastObservedAt on every scan
        asset.LastObservedAt = DateTime.UtcNow;
        context.InventoryAssets.Update(asset);
        await context.SaveChangesAsync();

        // Reload session with all navigation properties
        var updated = await context.InventoryBorrows
            .Include(b => b.Items).ThenInclude(i => i.Asset)
            .Include(b => b.BorrowedByUser)
            .FirstAsync(b => b.Id == id);

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> RemoveScannedItem(Guid id, Guid itemId)
    {
        var item = await context.InventoryBorrowItems.FirstOrDefaultAsync(i => i.Id == itemId && i.BorrowId == id);
        if (item == null)
        {
            return NotFound();
        }

        context.InventoryBorrowItems.Remove(item);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<BorrowSessionDto>> CompleteSession(Guid id)
    {
        var session = await context.InventoryBorrows
            .Include(b => b.Items)
            .ThenInclude(i => i.Asset)
            .Include(b => b.BorrowedByUser)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (session == null)
        {
            return NotFound();
        }

        if (session.CompletedAt.HasValue)
        {
            return BadRequest("Session is already completed");
        }

        session.CompletedAt = DateTime.UtcNow;

        // Update asset state based on borrow type
        if (session.Type == Data.Inventory.BorrowType.Borrow)
        {
            // Set CurrentBorrowId for all assets in this borrow
            foreach (var item in session.Items)
            {
                item.Asset.CurrentBorrowId = session.Id;
                item.Asset.UpdatedAt = DateTime.UtcNow;
            }
        }
        else if (session.Type == Data.Inventory.BorrowType.Return)
        {
            // Clear CurrentBorrowId for all assets in this return
            foreach (var item in session.Items)
            {
                item.Asset.CurrentBorrowId = null;
                item.Asset.UpdatedAt = DateTime.UtcNow;
            }
        }
        // InventoryCheck type doesn't modify CurrentBorrowId

        context.InventoryBorrows.Update(session);
        await context.SaveChangesAsync();

        return Ok(MapToDto(session));
    }

    private BorrowSessionDto MapToDto(InventoryBorrow borrow)
    {
        return new BorrowSessionDto
        {
            Id = borrow.Id,
            BorrowedByUserId = borrow.BorrowedByUserId,
            BorrowedByUserName = borrow.BorrowedByUser?.UserName ?? "Unknown",
            EventName = borrow.EventName,
            Type = borrow.Type.ToString(),
            StartedAt = borrow.StartedAt,
            CompletedAt = borrow.CompletedAt,
            Items = borrow.Items.Select(i => new BorrowItemDto
            {
                Id = i.Id,
                AssetId = i.AssetId,
                Tag = i.Asset?.Tag ?? "",
                Beskrivelse = i.Asset?.Beskrivelse ?? "",
                ScannedAt = i.ScannedAt
            }).ToList()
        };
    }
}
