using CrawlProject.Dto;
using HtmlAgilityPack;

namespace CrawlProject.Interfaces.Services;

public interface ICrawlService
{
    Task<List<Dictionary<string, object>>> CrawlBasicData(List<HtmlNode> TourNodes,
        CrawlDataRequestDto crawlDataRequestDto, string url);

    Task<List<Dictionary<string, object>>> CrawlData(CrawlDataRequestDto crawlDataRequestDto);
}