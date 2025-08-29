using System.ComponentModel;
using CrawlProject.Interfaces.Services;
using OfficeOpenXml;

namespace CrawlProject.Services;

public class ExcelService : IExcelService
{
    public byte[] GenerateExcel(List<Dictionary<string, object>> data)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Results");

            if (data.Count > 0)
            {
                var headers = data[0].Keys.ToList();
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Column(i + 1).AutoFit();
                }

                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < headers.Count; col++)
                    {
                        var value = data[row][headers[col]];
                        if (value is IEnumerable<string> arr)
                            worksheet.Cells[row + 2, col + 1].Value = string.Join("\n", arr);
                        else
                            worksheet.Cells[row + 2, col + 1].Value = value;

                        worksheet.Cells[row + 2, col + 1].Style.WrapText = true;
                    }
                }
            }

            return package.GetAsByteArray();
        }
    }
}