using Microsoft.AspNetCore.Mvc;
using CrawlProject.Interfaces.Services;
using CrawlProject.Core.Constants;
using CrawlProject.Dto;
using CrawlProject.Dto.Response;
using HtmlAgilityPack;

namespace CrawlProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrawlController : ControllerBase
{
    private readonly IWebContentExtractionService _contentExtractionService;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<CrawlController> _logger;
    private readonly ICrawlService _crawlService;
    private readonly ILinkDiscoveryService _linkDiscoveryService;
    private readonly IExcelService _excelService;

    public CrawlController(IWebContentExtractionService contentExtractionService, IGeminiService geminiService,
        ILogger<CrawlController> logger, ICrawlService crawlService, ILinkDiscoveryService linkDiscoveryService,
        IExcelService excelService)
    {
        _contentExtractionService = contentExtractionService;
        _geminiService = geminiService;
        _logger = logger;
        _crawlService = crawlService;
        _linkDiscoveryService = linkDiscoveryService;
        _excelService = excelService;
    }

    [HttpPost]
    [Route("CrawlData")]
    public async Task<IActionResult> CrawlData([FromBody] CrawlDataRequestDto request)
    {
        List<Dictionary<string, object>> data = await _crawlService.CrawlData(request);
        if (data != null && data.Count > 0)
        {
            byte[] excelBytes = _excelService.GenerateExcel(data);
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "CrawledData.xlsx");
        }

        throw new InvalidOperationException("No data found to crawl.");
    }

    [HttpPost]
    [Route("CustomSelect")]
    public async Task<IActionResult> CustomSelect([FromBody] CustomSelectRequestDto request)
    {
        CustomSelectResponseDto customSelectResponseDto =
            await _contentExtractionService.GetXpathOfCustomSelect(request);
        return Ok(customSelectResponseDto);
    }

    [HttpPost]
    [Route("GetSelects")]
    public async Task<IActionResult> GetSelectFromUrl([FromBody] GetSelectRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(CrawlConstants.ValidationMessages.UrlRequired);

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return BadRequest(CrawlConstants.ValidationMessages.InvalidUrlFormat);

        _logger.LogInformation(CrawlConstants.LogMessages.StartingAnalysis, request.Url);

        var bodyContent = await _contentExtractionService.GetBodyContentByUrl(request.Url, true);
        string? parentXpath = null;
        HtmlDocument? htmlFromXPath = null;
        
        try
        {
            _logger.LogInformation(CrawlConstants.LogMessages.AnalyzingWithAI, request.Url);
            parentXpath = await _geminiService.GetParentSelectorAsync(bodyContent);
            
            if (!string.IsNullOrWhiteSpace(parentXpath))
            {
                parentXpath = parentXpath.Replace("\n", "").Replace("\r", "").Trim();
                _logger.LogInformation("Received xPath from AI: {Selector}", parentXpath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, CrawlConstants.LogMessages.AIAnalysisFailed, request.Url);
        }

        if (!string.IsNullOrWhiteSpace(parentXpath))
        {
            try
            {
                _logger.LogInformation(CrawlConstants.LogMessages.ExtractingByXPath, parentXpath);
                htmlFromXPath = await _contentExtractionService.GetHtmlByXpath(bodyContent, parentXpath, request.Url);

                var selectorHtmlString = htmlFromXPath?.DocumentNode?.OuterHtml ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(selectorHtmlString))
                {
                    _logger.LogInformation("Successfully extracted HTML by XPath, length: {Length}",
                        selectorHtmlString.Length);
                }
                else
                {
                    _logger.LogWarning("Selector extraction returned empty result, using full HTML as fallback");
                    htmlFromXPath = new HtmlDocument();
                    htmlFromXPath.LoadHtml(bodyContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, CrawlConstants.LogMessages.XPathExtractionFailed,
                    parentXpath, request.Url);
                htmlFromXPath = new HtmlDocument();
                htmlFromXPath.LoadHtml(bodyContent);
            }
        }
        else
        {
            _logger.LogInformation("No Xpath available, returning full body content");
            htmlFromXPath = new HtmlDocument();
            htmlFromXPath.LoadHtml(bodyContent);
        }

        if (htmlFromXPath?.DocumentNode == null)
        {
            _logger.LogError("Failed to load HTML document from XPath, returning empty response");
            throw new InvalidOperationException("Failed to load HTML document from XPath.");
        }

        var selectResponseDto = await _linkDiscoveryService.DiscoverAndProcessLinksAsync(
            request.Url, bodyContent, parentXpath, htmlFromXPath);

        return Ok(selectResponseDto);
    }
}