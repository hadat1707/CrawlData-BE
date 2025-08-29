using CrawlProject.Dto;
using CrawlProject.Dto.Response;
using HtmlAgilityPack;

namespace CrawlProject.Interfaces.Services;

public interface IWebContentExtractionService
{
    Task<string> GetHtmlAsync(string url);
    Task<HtmlDocument> GetHtmlByXpath(string fullHtml, string xPath, string url);
    string ExtractBodyContent(string html);
    Task<string> CheckProgramExists(List<string> urlDetails);
    Task<string> RemoveElementByXPath(string html, Dictionary<string, string> xpathPrograms);

    Task<string> GetBodyContentByUrl(string url, bool isClean);

    Task<Dictionary<string, string>> GetOption(string html, string tourXpath);

    Task<Dictionary<string, string>> GetUrlPagination(string url, string paginationXpath);

    Task<List<HtmlNode>> ExtractNodesByXpath(string html, string xpath);

    Task<CustomSelectResponseDto> GetXpathOfCustomSelect(CustomSelectRequestDto customSelectRequestDto);

    Task<string> GetBodyDetail(string url);

    Task<string> GetPopupXpath(string htmlOriginal, string htmlDisplayedPopup);
}