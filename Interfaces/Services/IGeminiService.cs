namespace CrawlProject.Interfaces.Services;

public interface IGeminiService
{
    Task<string> GetParentSelectorAsync(string html);
    Task<Dictionary<string, string>> GetParentTourProgramXpathAsync(string html);
    Task<Dictionary<string, string>> GetTourDetailSelectorsAsync(string html);
    Task<Dictionary<string, string>> GetXpathBasicSummaryAndParent(string html);

    Task<Dictionary<string, string>> GetXpathOfHtmlByRequest(string html, string request);

    Task<Dictionary<string, string>> GetOptionXpath(string html);

    Task<List<string>> GetPeripheralWrappersXpathListAsync(string html);
}