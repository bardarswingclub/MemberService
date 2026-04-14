namespace MemberService.Tests.Inventory;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;
using MemberService.Data;
using MemberService.Data.Inventory;
using MemberService.Pages.Inventory;

[TestFixture]
public class BorrowSessionLogicTests
{
    private static readonly string TestUserId = "user-1";

    private static async Task<MemberContext> CreateContext()
    {
        var ctx = new MemberContext(new DbContextOptionsBuilder<MemberContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        // BorrowedByUser navigation is included in the reload query — seed a minimal user
        ctx.Users.Add(new User
        {
            Id = TestUserId,
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString(),
        });
        await ctx.SaveChangesAsync();
        return ctx;
    }

    private static InventoryAsset MakeAsset(string tag) => new()
    {
        Id = Guid.NewGuid(),
        Tag = tag,
        Beskrivelse = tag,
        InInventory = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    private static InventoryBorrow MakeSession(bool completed = false) => new()
    {
        Id = Guid.NewGuid(),
        BorrowedByUserId = TestUserId,
        EventName = "Test",
        Type = BorrowType.Borrow,
        StartedAt = DateTime.UtcNow,
        CompletedAt = completed ? DateTime.UtcNow : null,
    };

    [Test]
    public async Task ScanIntoCompletedSession_Returns400()
    {
        await using var ctx = await CreateContext();
        var asset = MakeAsset("S-001");
        var session = MakeSession(completed: true);
        ctx.InventoryAssets.Add(asset);
        ctx.InventoryBorrows.Add(session);
        await ctx.SaveChangesAsync();

        var controller = new InventoryBorrowsController(ctx);
        var result = await controller.ScanAsset(session.Id, new ScanAssetRequest { Tag = "S-001" });

        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task DoubleScanSameTag_NoopNoDuplicateRow()
    {
        await using var ctx = await CreateContext();
        var asset = MakeAsset("S-001");
        var session = MakeSession();
        ctx.InventoryAssets.Add(asset);
        ctx.InventoryBorrows.Add(session);
        await ctx.SaveChangesAsync();

        var controller = new InventoryBorrowsController(ctx);

        await controller.ScanAsset(session.Id, new ScanAssetRequest { Tag = "S-001" });
        await controller.ScanAsset(session.Id, new ScanAssetRequest { Tag = "S-001" });

        ctx.InventoryBorrowItems.Count(i => i.BorrowId == session.Id).ShouldBe(1);
    }

    [Test]
    public async Task BorrowComplete_SetsCurrentBorrowId_ReturnComplete_ClearsIt()
    {
        await using var ctx = await CreateContext();
        var asset = MakeAsset("S-001");
        var borrowSession = new InventoryBorrow
        {
            Id = Guid.NewGuid(),
            BorrowedByUserId = "user-1",
            EventName = "Test",
            Type = BorrowType.Borrow,
            StartedAt = DateTime.UtcNow,
        };
        ctx.InventoryAssets.Add(asset);
        ctx.InventoryBorrows.Add(borrowSession);
        await ctx.SaveChangesAsync();

        var controller = new InventoryBorrowsController(ctx);

        // Scan and complete borrow
        await controller.ScanAsset(borrowSession.Id, new ScanAssetRequest { Tag = "S-001" });
        await controller.CompleteSession(borrowSession.Id);

        ctx.InventoryAssets.Single(a => a.Tag == "S-001").CurrentBorrowId.ShouldBe(borrowSession.Id);

        // Now return
        var returnSession = new InventoryBorrow
        {
            Id = Guid.NewGuid(),
            BorrowedByUserId = TestUserId,
            EventName = "Test",
            Type = BorrowType.Return,
            StartedAt = DateTime.UtcNow,
        };
        ctx.InventoryBorrows.Add(returnSession);
        await ctx.SaveChangesAsync();

        await controller.ScanAsset(returnSession.Id, new ScanAssetRequest { Tag = "S-001" });
        await controller.CompleteSession(returnSession.Id);

        ctx.InventoryAssets.Single(a => a.Tag == "S-001").CurrentBorrowId.ShouldBeNull();
    }
}
