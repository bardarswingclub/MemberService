namespace MemberService.Tests.Inventory;

using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;
using MemberService.Data;
using MemberService.Pages.Inventory;

[TestFixture]
public class CsvImportServiceTests
{
    private MemberContext CreateContext() =>
        new(new DbContextOptionsBuilder<MemberContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    // CSV column order: No, Tag, Inventory, Lokasjon, Kategori, Sub-kategori, Beskrivelse, Merke, Modell, Detaljer, Lengde [m], Diameter
    private static string MakeRow(string no, string tag, string inventory = "1", string lokasjon = "", string kategori = "", string subKategori = "", string beskrivelse = "Test", string merke = "", string modell = "", string detaljer = "", string lengde = "", string diameter = "")
        => $"{no},{tag},{inventory},{lokasjon},{kategori},{subKategori},{beskrivelse},{merke},{modell},{detaljer},{lengde},{diameter}";

    private const string Header = "No,Tag,Inventory,Lokasjon,Kategori,Sub-kategori,Beskrivelse,Merke,Modell,Detaljer,Lengde [m],Diameter";

    [Test]
    public async Task ValidImport_3New2Updates()
    {
        await using var ctx = CreateContext();

        // Pre-seed two assets that will be updated
        ctx.InventoryAssets.AddRange(
            new Data.Inventory.InventoryAsset { Id = Guid.NewGuid(), Tag = "K-001", Beskrivelse = "Old", InInventory = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Data.Inventory.InventoryAsset { Id = Guid.NewGuid(), Tag = "K-002", Beskrivelse = "Old", InInventory = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await ctx.SaveChangesAsync();

        var csv = string.Join("\n", new[]
        {
            Header,
            MakeRow("1", "K-001", beskrivelse: "Kabel 1", merke: "Neutrik"),
            MakeRow("2", "K-002", beskrivelse: "Kabel 2"),
            MakeRow("3", "S-001", beskrivelse: "Stativ 1"),
            MakeRow("4", "S-002", beskrivelse: "Stativ 2"),
            MakeRow("5", "M-001", beskrivelse: "Mikrofon 1"),
        });

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(5);
        result.ErrorCount.ShouldBe(0);
        ctx.InventoryAssets.Count().ShouldBe(5);

        var k001 = ctx.InventoryAssets.Single(a => a.Tag == "K-001");
        k001.Beskrivelse.ShouldBe("Kabel 1");
        k001.Merke.ShouldBe("Neutrik");
    }

    [Test]
    public async Task MissingTag_RowError_OthersSucceed()
    {
        await using var ctx = CreateContext();

        var csv = string.Join("\n", new[]
        {
            Header,
            MakeRow("1", "S-001", beskrivelse: "Stativ OK"),
            MakeRow("2", "",     beskrivelse: "Uten tag"),   // empty tag → error
            MakeRow("3", "S-002", beskrivelse: "Stativ OK"),
        });

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(2);
        result.ErrorCount.ShouldBe(1);
        result.Errors.Single().RowNumber.ShouldBe(3); // row 1 = header, row 2 = S-001, row 3 = empty tag
        ctx.InventoryAssets.Count().ShouldBe(2);
    }

    [Test]
    public async Task QuotedDecimalComma_ParsedCorrectly()
    {
        await using var ctx = CreateContext();

        // "2,5" in a CSV field — quoted because the comma would otherwise split the field
        // Columns: No(0), Tag(1), Inventory(2), Lokasjon(3), Kategori(4), Sub-kategori(5), Beskrivelse(6), Merke(7), Modell(8), Detaljer(9), Lengde[m](10), Diameter(11)
        // Need 4 commas after Beskrivelse to reach index 10: Merke(7), Modell(8), Detaljer(9) = 3 empty + comma into field 10
        var csv = $"{Header}\n1,K-003,1,,,,Kabel,,,,\"2,5\",";

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(1);
        result.ErrorCount.ShouldBe(0);
        ctx.InventoryAssets.Single(a => a.Tag == "K-003").LengdeM.ShouldBe(2.5m);
    }

    [Test]
    public async Task WindowsLineEndings_AllRowsImported()
    {
        await using var ctx = CreateContext();

        var csv = Header + "\r\n"
            + MakeRow("1", "S-001", beskrivelse: "Stativ") + "\r\n"
            + MakeRow("2", "S-002", beskrivelse: "Stativ") + "\r\n";

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(2);
        result.ErrorCount.ShouldBe(0);
    }

    [Test]
    public async Task EmptyRows_Skipped()
    {
        await using var ctx = CreateContext();

        var csv = string.Join("\n", new[]
        {
            Header,
            MakeRow("1", "S-001", beskrivelse: "Stativ"),
            "",
            "   ",
            MakeRow("2", "S-002", beskrivelse: "Stativ"),
            "",
        });

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(2);
        result.ErrorCount.ShouldBe(0);
        ctx.InventoryAssets.Count().ShouldBe(2);
    }

    [Test]
    public async Task HeaderRow_Skipped()
    {
        await using var ctx = CreateContext();

        // Only header + one real row
        var csv = Header + "\n" + MakeRow("1", "S-001", beskrivelse: "Stativ");

        var svc = new CsvImportService(ctx);
        var result = await svc.ImportAsync(csv);

        result.SuccessCount.ShouldBe(1);
        // Header must not have been imported as an asset
        ctx.InventoryAssets.Any(a => a.Tag == "Tag").ShouldBeFalse();
    }
}
