using CrawlProject.Dto.Response;

namespace CrawlProject.Interfaces.Services;

public interface IUtilsService
{
    string getXpathDyanamic(string xPath);
    List<string> HandleUrl(string urlOrigin, List<Dictionary<string, object>> urlTourDetails);
    void CompareElement(Dictionary<string, string> infoXpath, Dictionary<string, string> basicInfoDetailXpath);

    void CleanElement(SelectResponseDto selectResponseDto);

    string CheckAndAppendUrl(string url, string urlOrigin);
}