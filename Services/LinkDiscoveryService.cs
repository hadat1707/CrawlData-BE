using HtmlAgilityPack;
using CrawlProject.Interfaces.Services;
using CrawlProject.Dto.Response;
using CrawlProject.Core.Constants;

namespace CrawlProject.Services;

public class LinkDiscoveryService : ILinkDiscoveryService
{
    private readonly IWebContentExtractionService _contentExtractionService;
    private readonly IGeminiService _geminiService;
    private readonly IHeuristicService _heuristicService;
    private readonly IUtilsService _utilsService;
    private readonly ILogger<LinkDiscoveryService> _logger;
    private readonly IChromeWebDriverService _webDriverService;

    public LinkDiscoveryService(
        IWebContentExtractionService contentExtractionService,
        IGeminiService geminiService,
        IHeuristicService heuristicService,
        IUtilsService utilsService,
        ILogger<LinkDiscoveryService> logger,
        IChromeWebDriverService webDriverService)
    {
        _contentExtractionService = contentExtractionService;
        _geminiService = geminiService;
        _heuristicService = heuristicService;
        _utilsService = utilsService;
        _logger = logger;
        _webDriverService = webDriverService;
    }


    public async Task<SelectResponseDto> DiscoverAndProcessLinksAsync(string url, string bodyContent,
        string parentXpath, HtmlDocument htmlFromXPath)
    {
        try
        {
            HtmlDocument _htmlFullLoaded = new HtmlDocument();
            HtmlDocument _htmlFullPagination = new HtmlDocument();
            Dictionary<string, string> loaderOption = new Dictionary<string, string>();
            Dictionary<string, string> paginationOption = new Dictionary<string, string>();

            Dictionary<string, string>
                optionXpath = await _contentExtractionService.GetOption(bodyContent, parentXpath);

            if (optionXpath.ContainsKey("loader"))
            {
                string html = await _webDriverService.GetHtmlWithLoadMoreAsync(url, optionXpath["loader"]);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                _htmlFullLoaded = await _contentExtractionService.GetHtmlByXpath(html, parentXpath, null);
                loaderOption = optionXpath;
            }
            else if (optionXpath.ContainsKey("pagination"))
            {
                Dictionary<string, string> paginationUrls =
                    await _contentExtractionService.GetUrlPagination(url, optionXpath["pagination"]);

                string htmlAppended = string.Empty;
                foreach (var paginationUrl in paginationUrls)
                {
                    var content =
                        await _contentExtractionService.GetHtmlByXpath(null, parentXpath, paginationUrl.Value);
                    htmlAppended += content.DocumentNode.OuterHtml;
                }

                _htmlFullPagination.LoadHtml(htmlAppended);
                paginationOption = paginationUrls;
            }

            HtmlDocument targetHtml;
            if (optionXpath.ContainsKey("loader") && _htmlFullLoaded?.DocumentNode != null)
            {
                targetHtml = _htmlFullLoaded;
            }
            else if (optionXpath.ContainsKey("pagination") && _htmlFullPagination?.DocumentNode != null)
            {
                targetHtml = _htmlFullPagination;
            }
            else
            {
                targetHtml = htmlFromXPath;
            }

            var urlDetailNodes = _heuristicService
                .ScrapeDynamicData(null, targetHtml, parentXpath, CrawlConstants.ContentTypes.Link)
                .Select(dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                .ToList();

            var urlDetails = _utilsService.HandleUrl(url, urlDetailNodes);


            Dictionary<string, string> xpathPrograms = new Dictionary<string, string>();

            var programExistsHtml = await _contentExtractionService.CheckProgramExists(urlDetails);
            if (!String.IsNullOrEmpty(programExistsHtml))
            {
                xpathPrograms = await _geminiService.GetParentTourProgramXpathAsync(programExistsHtml);
            }

            var htmlRemovedProgram =
                await _contentExtractionService.RemoveElementByXPath(programExistsHtml, xpathPrograms);

            if (String.IsNullOrEmpty(htmlRemovedProgram))
            {
                htmlRemovedProgram = await _contentExtractionService.GetBodyDetail(urlDetails[0]);
            }

            var infoXpath = await _geminiService.GetTourDetailSelectorsAsync(htmlFromXPath.Text);
            var basicInfoDetailXpath = await _geminiService.GetXpathBasicSummaryAndParent(htmlRemovedProgram);

            if (infoXpath.Count != 0 || basicInfoDetailXpath.Count != 0)
            {
                _utilsService.CompareElement(infoXpath, basicInfoDetailXpath);
            }

            infoXpath.Add("ParentList", parentXpath);

            var selectResponseDto = new SelectResponseDto
            {
                Elements = infoXpath,
                Detail = new Detail()
                {
                    Basic = new Basic()
                    {
                        Elements = basicInfoDetailXpath
                    },
                    _Program = new _Program()
                    {
                        Elements = xpathPrograms
                    }
                }
            };
            _utilsService.CleanElement(selectResponseDto);

            if (loaderOption.Count > 0)
                selectResponseDto.Options = new Options
                {
                    Loader = loaderOption
                };
            else if (paginationOption.Count > 0)
                selectResponseDto.Options = new Options
                {
                    Pagination = paginationOption
                };


            return selectResponseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DiscoverAndProcessLinksAsync");
            return null;
        }
    }
}