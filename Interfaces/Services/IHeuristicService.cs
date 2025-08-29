using HtmlAgilityPack;

namespace CrawlProject.Interfaces.Services;

public interface IHeuristicService
{
    Task<string> ExistsElementByElementDefinitions(List<string> urlDetails);
    List<Dictionary<string, object>> ScrapeDynamicData(string? selector, HtmlDocument htmlDoc, string? parentXpath, string type);
}
