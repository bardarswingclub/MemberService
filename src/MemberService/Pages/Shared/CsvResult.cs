namespace MemberService.Pages.Shared;

using System.Text;

using Microsoft.AspNetCore.Mvc;

public class CsvResult : FileContentResult
{
    public CsvResult(string fileContents, string fileName = "export.csv") : base(ToBytes(fileContents), "text/csv; charset=utf-8")
    {
        FileDownloadName = fileName;
    }

    public static CsvResult Create<T>(IEnumerable<T> rows, string fileName = "export.csv")
    {
        return new CsvResult(rows.ToCsv(), fileName);
    }

    private static byte[] ToBytes(string fileContents) => Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(fileContents)).ToArray();
}
