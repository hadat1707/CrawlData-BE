using CrawlProject.Dto;
using CrawlProject.Dto.Response;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using CrawlProject.Utils;
using CrawlProject.Interfaces.Services;
using HtmlAgilityPack;

namespace CrawlProject.Services;

public class WebContentExtractionService : IWebContentExtractionService
{
    private readonly ILogger<WebContentExtractionService> _logger;
    private readonly IUtilsService _utilsService;
    private readonly HandleHtml _handleHtml;
    private readonly IChromeWebDriverService _chromeWebDriverService;
    private readonly Lazy<IHeuristicService> _heuristicService;
    private readonly IGeminiService _geminiService;

    public WebContentExtractionService(ILogger<WebContentExtractionService> logger, IUtilsService utilsService,
        HandleHtml handleHtml,
        IChromeWebDriverService chromeWebDriverService, Lazy<IHeuristicService> heuristicService,
        IGeminiService geminiService)
    {
        _logger = logger;
        _utilsService = utilsService;
        _handleHtml = handleHtml;
        _chromeWebDriverService = chromeWebDriverService;
        _heuristicService = heuristicService;
        _geminiService = geminiService;
    }

    public async Task<string> GetHtmlAsync(string url)
    {
        var attempts = new Func<Task<string>>[]
        {
            async () => await _chromeWebDriverService.GetHtmlFastAsync(url),
            async () => await _chromeWebDriverService.GetHtmlStealthAsync(url),
            async () => await _chromeWebDriverService.GetHtmlNonHeadlessAsync(url),
            async () => await _chromeWebDriverService.GetHtmlMinimalAsync(url)
        };

        Exception? lastException = null;

        foreach (var attempt in attempts)
        {
            try
            {
                var result = await attempt();
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            catch (WebDriverTimeoutException ex)
            {
                lastException = ex;
                _logger.LogWarning("WebDriver timeout for {Url}, trying next approach: {Message}", url, ex.Message);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt failed for {Url}, trying next approach", url);
            }
        }

        throw lastException ?? new Exception("All HTML retrieval attempts failed");
    }

    public async Task<string> GetPopupXpath(string htmlOriginal, string htmlDisplayedPopup)
    {
        var docOriginal = new HtmlDocument();
        docOriginal.LoadHtml(htmlOriginal);

        var docPopup = new HtmlDocument();
        docPopup.LoadHtml(htmlDisplayedPopup);

        var originalNodes = new HashSet<string>(
            docOriginal.DocumentNode.Descendants()
                .Select(n => n.XPath)
        );

        foreach (var node in docPopup.DocumentNode.Descendants())
        {
            if (!originalNodes.Contains(node.XPath))
            {
                return node.XPath;
            }
        }

        return string.Empty;
    }
    
    public async Task<CustomSelectResponseDto> GetXpathOfCustomSelect(CustomSelectRequestDto customSelectRequestDto)
    {
        // ALERT: This method is currently being upgraded and optimized.
        return null;
    }
    
    public async Task<Dictionary<string, string>> GetOption(string html, string tourXpath)
    {
        string contentRemovedTour = RemoveToursByXpath(html, tourXpath);
        Dictionary<string, string> optionXpath = await _geminiService.GetOptionXpath(contentRemovedTour);
        if (optionXpath != null && optionXpath.Count > 0)
        {
            _logger.LogInformation("Successfully extracted options from HTML using Gemini service.");
            return optionXpath;
        }

        _logger.LogWarning("No options found in the HTML content using Gemini service.");
        return new Dictionary<string, string>();
    }


    private string RemoveToursByXpath(string html, string tourXpath)
    {
        ValidationHelper.ValidateNotNullOrEmpty(html, nameof(html));
        ValidationHelper.ValidateNotNullOrEmpty(tourXpath, nameof(tourXpath));
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        try
        {
            var nodes = doc.DocumentNode.SelectNodes(tourXpath);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tours by XPath: {XPath}", tourXpath);
        }

        return doc.DocumentNode.OuterHtml;
    }

    public async Task<string> GetBodyDetail(string url)
    {
        string html = await GetBodyContentByUrl(url, true);
        return _handleHtml.CleanHtmlTourDetail(html);
    }

    public async Task<string> RemoveElementByXPath(string html, Dictionary<string, string> xpathPrograms)
    {
        if (string.IsNullOrEmpty(html) || xpathPrograms.Count == 0)
        {
            _logger.LogWarning("HTML content or XPath programs are null or empty.");
            return html;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var xpath in xpathPrograms.Values)
        {
            try
            {
                var nodes = doc.DocumentNode.SelectNodes(xpath);
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        node.Remove();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing elements by XPath: {XPath}", xpath);
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    public async Task<string> CheckProgramExists(List<string> urlDetails)
    {
        if (urlDetails == null || urlDetails.Count == 0)
        {
            _logger.LogWarning("No URLs provided to check for schedules.");
            return string.Empty;
        }

        string bodyContent = await _heuristicService.Value.ExistsElementByElementDefinitions(urlDetails);

        if (!string.IsNullOrEmpty(bodyContent))
        {
            string htmlCleaned = _handleHtml.CleanHtmlTourDetail(bodyContent);
            return _handleHtml.RemoveSpace(htmlCleaned);
        }

        _logger.LogInformation("No Programs found in the provided URLs.");
        return string.Empty;
    }

    public async Task<string> GetBodyContentByUrl(string url, bool isClean)
    {
        try
        {
            string htmlRaw = await GetHtmlAsync(url);
            if (string.IsNullOrEmpty(htmlRaw))
            {
                _logger.LogWarning("No HTML content retrieved for URL: {Url}", url);
                return string.Empty;
            }

            string bodyContent = isClean ? ExtractBodyContent(htmlRaw) : ExtractBodyContent_NotClean(htmlRaw);

            // bodyContent = await _handleHtml.CleanDynamic(bodyContent);

            bodyContent = bodyContent?.Replace("\n", "").Replace("\r", "");

            bodyContent = System.Text.RegularExpressions.Regex.Replace(bodyContent ?? "", @"\s+", " ");

            if (string.IsNullOrEmpty(bodyContent))
            {
                _logger.LogWarning("No body content found in HTML for URL: {Url}", url);
                return string.Empty;
            }

            _logger.LogInformation("Successfully retrieved body content for URL: {Url}", url);
            return bodyContent;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve HTML content for URL: {Url}", url);
            return string.Empty;
        }
    }

    public async Task<Dictionary<string, string>> GetUrlPagination(string url, string paginationXpath)
    {
        string htmlRaw = await GetBodyContentByUrl(url, true);

        if (string.IsNullOrEmpty(htmlRaw))
        {
            _logger.LogWarning("No HTML content retrieved for URL: {Url}", url);
            throw new InvalidOperationException($"Failed to retrieve HTML content for URL '{url}'");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlRaw);

        var paginationNodes = doc.DocumentNode.SelectNodes(paginationXpath);
        if (paginationNodes == null || paginationNodes.Count == 0)
        {
            _logger.LogWarning("No pagination nodes found for XPath: {XPath} in URL: {Url}", paginationXpath, url);
            throw new InvalidOperationException(
                $"No pagination nodes found for XPath '{paginationXpath}' in URL '{url}'");
        }

        var paginationUrls = new Dictionary<string, string>();
        foreach (var node in paginationNodes)
        {
            var aTags = node.SelectNodes(".//a");
            if (aTags == null) continue;

            foreach (var aTag in aTags)
            {
                var linkText = aTag.InnerText.Trim().ToLower();
                var rel = aTag.GetAttributeValue("rel", string.Empty).ToLower();
                var ariaLabel = aTag.GetAttributeValue("aria-label", string.Empty).ToLower();

                if (linkText.Contains("prev") || linkText.Contains("next") || linkText.Contains("trước") ||
                    linkText.Contains("sau") || linkText.Contains(">>") || ariaLabel.Contains("prev") ||
                    ariaLabel.Contains("next") || linkText.Contains("<<") ||
                    rel == "prev" || rel == "next")
                    continue;

                var href = aTag.GetAttributeValue("href", string.Empty);
                var pageNumber = aTag.InnerText.Trim();

                if (!string.IsNullOrEmpty(pageNumber))
                {
                    if (string.IsNullOrEmpty(href))
                    {
                        paginationUrls.TryAdd(pageNumber, url);
                    }
                    else
                    {
                        var fullUrl = _utilsService.CheckAndAppendUrl(href, url);
                        paginationUrls.TryAdd(pageNumber, fullUrl);
                    }
                }
            }
        }

        if (paginationUrls.Count == 0)
        {
            _logger.LogWarning("No valid pagination URLs found in the HTML content for URL: {Url}", url);
            throw new InvalidOperationException($"No valid pagination URLs found in the HTML content for URL '{url}'");
        }

        _logger.LogInformation("Found {Count} pagination URLs for XPath: {XPath} in URL: {Url}",
            paginationUrls.Count, paginationXpath, url);
        return paginationUrls;
    }

    public async Task<HtmlDocument> GetHtmlByXpath(string fullHtml, string xPath, string url)
    {
        try
        {
            if (!string.IsNullOrEmpty(fullHtml))
            {
                _logger.LogInformation("Attempting lightweight HTML extraction for xPath: {xPath}", xPath);

                if (!string.IsNullOrEmpty(fullHtml))
                {
                    var extractedHtml = ExtractByXpath(fullHtml, xPath);
                    extractedHtml = _handleHtml.CleanHtmlInsideParentXpath(extractedHtml);
                    extractedHtml = _handleHtml.RemoveSpace(extractedHtml);
                    if (!string.IsNullOrEmpty(extractedHtml))
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(extractedHtml);
                        return doc;
                    }
                    else
                    {
                        _logger.LogWarning("XPath not found in HTML, falling back to Chrome-based extraction");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lightweight approach failed, trying Chrome-based extraction");
        }


        try
        {
            if (!string.IsNullOrEmpty(url))
            {
                _logger.LogInformation(
                    "Chrome-based extraction fallback using GetBodyContentByUrl for XPath: {xpath} from URL: {Url}",
                    xPath,
                    url);

                var bodyContent = await GetBodyContentByUrl(url, true);

                if (string.IsNullOrEmpty(bodyContent))
                {
                    _logger.LogWarning("No body content retrieved for URL: {Url}, returning empty document",
                        url);
                    return new HtmlDocument();
                }

                var extractedHtml = ExtractByXpath(bodyContent, xPath);

                if (!string.IsNullOrEmpty(extractedHtml))
                {
                    extractedHtml = _handleHtml.CleanHtmlInsideParentXpath(extractedHtml);
                    extractedHtml = _handleHtml.RemoveSpace(extractedHtml);

                    _logger.LogInformation(
                        "Successfully extracted HTML by XPath: {xpath} using GetBodyContentByUrl",
                        xPath);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(extractedHtml);
                    return doc;
                }
                else
                {
                    _logger.LogWarning("XPath: {xpath} not found in body content, returning empty document",
                        xPath);
                    throw new InvalidOperationException(
                        $"XPath '{xPath}' not found in body content from URL '{url}'");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Chrome-based extraction using GetBodyContentByUrl failed for XPath: {xpath} from URL: {Url}",
                xPath,
                url);
            throw new InvalidOperationException(
                $"Failed to extract HTML by XPath '{xPath}' from URL '{url}': {ex.Message}", ex);
        }

        throw new InvalidOperationException(
            $"Failed to extract HTML by XPath '{xPath}' from URL '{url}': No content found");
    }

    public string ExtractBodyContent_NotClean(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var mainNode = doc.DocumentNode.SelectSingleNode("//main");
        if (mainNode != null)
            return mainNode.InnerHtml.Trim();

        var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
        return bodyNode != null ? bodyNode.InnerHtml.Trim() : doc.DocumentNode.OuterHtml.Trim();
    }

    public string ExtractBodyContent(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var mainNode = doc.DocumentNode.SelectSingleNode("//main | //*[@class='main'] | //*[@id='main']");
        if (mainNode != null)
        {
            var cleanedDoc = _handleHtml.CleanHtml(mainNode);
            return cleanedDoc.DocumentNode.InnerHtml.Trim();
        }

        var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
        if (bodyNode != null)
        {
            var cleanedDoc = _handleHtml.CleanHtml(bodyNode);
            return cleanedDoc.DocumentNode.InnerHtml.Trim();
        }

        return string.Empty;
    }

    public async Task<List<HtmlNode>> ExtractNodesByXpath(string html, string xpath)
    {
        ValidationHelper.ValidateNotNullOrEmpty(html, nameof(html));
        ValidationHelper.ValidateNotNullOrEmpty(xpath, nameof(xpath));
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes(xpath);
        if (nodes == null || nodes.Count == 0)
        {
            _logger.LogWarning("No nodes found for XPath: {XPath}", xpath);
            throw new InvalidOperationException(
                $"No nodes found for XPath '{xpath}' in the provided HTML content.");
        }

        _logger.LogInformation("Found {Count} nodes for XPath: {XPath}", nodes.Count, xpath);
        return nodes.ToList();
    }

    private string ExtractByXpath(string html, string xPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xPath))
                throw new ArgumentException("XPath cannot be null or empty.", nameof(xPath));

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode.SelectSingleNode(xPath);
            if (node != null)
                return node.OuterHtml;

            var xPathDynamic = _utilsService.getXpathDyanamic(xPath);
            if (xPathDynamic == null)
                throw new ArgumentException("Invalid XPath format. Expected format: //tag[@attribute='value']");

            node = doc.DocumentNode.SelectSingleNode(xPathDynamic);
            if (node != null)
                return node.OuterHtml;

            throw new InvalidOperationException($"XPath '{xPath}' not found in HTML");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error extracting HTML by XPath '{xPath}': {ex.Message}", ex);
        }
    }
}