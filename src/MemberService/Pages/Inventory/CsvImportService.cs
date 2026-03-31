namespace MemberService.Pages.Inventory;

using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using MemberService.Data.Inventory;

public class CsvImportService(MemberContext context)
{
    public async Task<CsvImportResult> ImportAsync(string csvContent)
    {
        var result = new CsvImportResult();
        var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        var rowNumber = 0;
        var isHeader = true;

        foreach (var line in lines)
        {
            rowNumber++;

            // Skip header
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var fields = ParseCsvLine(line);

                // Columns: No, Tag, Inventory, Lokasjon, Kategori, Sub-kategori, Beskrivelse, Merke, Modell, Detaljer, Lengde [m], Diameter
                if (fields.Count < 2)
                {
                    result.Errors.Add(new CsvImportError { RowNumber = rowNumber, Message = "Row has fewer than 2 fields" });
                    result.ErrorCount++;
                    continue;
                }

                var tag = fields[1]?.Trim();
                if (string.IsNullOrWhiteSpace(tag))
                {
                    result.Errors.Add(new CsvImportError { RowNumber = rowNumber, Message = "Tag field is empty" });
                    result.ErrorCount++;
                    continue;
                }

                var inventory = fields.Count > 2 && fields[2] == "1";
                var lokasjon = fields.Count > 3 ? fields[3]?.Trim() : null;
                var kategori = fields.Count > 4 ? fields[4]?.Trim() : null;
                var subKategori = fields.Count > 5 ? fields[5]?.Trim() : null;
                var beskrivelse = fields.Count > 6 ? fields[6]?.Trim() ?? "" : "";
                var merke = fields.Count > 7 ? fields[7]?.Trim() : null;
                var modell = fields.Count > 8 ? fields[8]?.Trim() : null;
                var detaljer = fields.Count > 9 ? fields[9]?.Trim() : null;
                var lengdeStr = fields.Count > 10 ? fields[10]?.Trim() : null;
                var diameterStr = fields.Count > 11 ? fields[11]?.Trim() : null;

                decimal? lengdeM = null;
                if (!string.IsNullOrWhiteSpace(lengdeStr))
                {
                    // Handle Norwegian decimal comma: "2,5" should become 2.5
                    var normalizedLengde = lengdeStr.Replace(",", ".");
                    if (decimal.TryParse(normalizedLengde, CultureInfo.InvariantCulture, out var parsedLength))
                    {
                        lengdeM = parsedLength;
                    }
                }

                int? diameter = null;
                if (!string.IsNullOrWhiteSpace(diameterStr) && int.TryParse(diameterStr, out var parsedDiameter))
                {
                    diameter = parsedDiameter;
                }

                // Upsert: find by Tag or create new
                var existing = await context.InventoryAssets.FirstOrDefaultAsync(a => a.Tag == tag);

                if (existing != null)
                {
                    existing.Kategori = kategori;
                    existing.SubKategori = subKategori;
                    existing.Beskrivelse = beskrivelse;
                    existing.Merke = merke;
                    existing.Modell = modell;
                    existing.Detaljer = detaljer;
                    existing.LengdeM = lengdeM;
                    existing.Diameter = diameter;
                    existing.InInventory = inventory;
                    existing.Lokasjon = lokasjon;
                    existing.UpdatedAt = DateTime.UtcNow;
                    context.InventoryAssets.Update(existing);
                }
                else
                {
                    var asset = new InventoryAsset
                    {
                        Id = Guid.NewGuid(),
                        Tag = tag,
                        Kategori = kategori,
                        SubKategori = subKategori,
                        Beskrivelse = beskrivelse,
                        Merke = merke,
                        Modell = modell,
                        Detaljer = detaljer,
                        LengdeM = lengdeM,
                        Diameter = diameter,
                        InInventory = inventory,
                        Lokasjon = lokasjon,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.InventoryAssets.Add(asset);
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new CsvImportError { RowNumber = rowNumber, Message = ex.Message });
                result.ErrorCount++;
            }
        }

        await context.SaveChangesAsync();
        return result;
    }

    private static List<string?> ParseCsvLine(string line)
    {
        var fields = new List<string?>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < line.Length)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    current.Append('"');
                    i += 2;
                }
                else
                {
                    // Toggle quote state
                    inQuotes = !inQuotes;
                    i++;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                fields.Add(current.ToString());
                current.Clear();
                i++;
            }
            else
            {
                current.Append(c);
                i++;
            }
        }

        // Add last field
        fields.Add(current.ToString());

        return fields;
    }
}
