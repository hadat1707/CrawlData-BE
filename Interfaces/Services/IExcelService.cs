namespace CrawlProject.Interfaces.Services;

public interface IExcelService
{
    public byte[] GenerateExcel(List<Dictionary<string, object>> data);
}