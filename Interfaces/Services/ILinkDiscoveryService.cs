using HtmlAgilityPack;
using CrawlProject.Dto.Response;

namespace CrawlProject.Interfaces.Services;

public interface ILinkDiscoveryService
{
    Task<SelectResponseDto> DiscoverAndProcessLinksAsync(string url, string bodyContent, string parentXpath, HtmlDocument htmlFromXPath);
}
