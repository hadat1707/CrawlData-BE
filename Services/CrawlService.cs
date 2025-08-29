using CrawlProject.Dto;
using CrawlProject.Interfaces.Services;
using CrawlProject.Utils;
using HtmlAgilityPack;

namespace CrawlProject.Services;

public class CrawlService : ICrawlService
{
    private readonly IWebContentExtractionService _webContentExtractionService;
    private readonly IChromeWebDriverService _webDriverService;
    private readonly IUtilsService _utilsService;
    private readonly IMongoDatabaseService _mongoDatabaseService;


    public CrawlService(IWebContentExtractionService webContentExtractionService,
        IChromeWebDriverService webDriverService,
        IUtilsService utilsService,
        IMongoDatabaseService mongoDatabaseService
    )
    {
        _webContentExtractionService = webContentExtractionService;
        _webDriverService = webDriverService;
        _utilsService = utilsService;
        _mongoDatabaseService = mongoDatabaseService;
    }


    public async Task<List<Dictionary<string, object>>> CrawlData(CrawlDataRequestDto crawlDataRequestDto)
    {
        ValidationHelper.ValidateNotNull(crawlDataRequestDto, nameof(crawlDataRequestDto));

        var url = crawlDataRequestDto.Url;
        var parentXpath = crawlDataRequestDto.Elements
            ?.FirstOrDefault(kvp => kvp.Key.Equals("Parent", StringComparison.OrdinalIgnoreCase)).Value;

        ValidationHelper.ValidateNotNullOrEmpty(url, nameof(crawlDataRequestDto.Url), "URL cannot be null or empty.");
        ValidationHelper.ValidateNotNullOrEmpty(parentXpath, nameof(crawlDataRequestDto.Elements),
            "Parent XPath is required.");

        List<HtmlNode> TourNodes = new List<HtmlNode>();

        if (crawlDataRequestDto.Options != null &&
            crawlDataRequestDto.Options.Loader != null &&
            crawlDataRequestDto.Options.Loader.Count > 0 &&
            crawlDataRequestDto.Options.Loader.ContainsKey("loader") &&
            !string.IsNullOrEmpty(crawlDataRequestDto.Options.Loader["loader"]))
        {
            string htmlFullLoaded =
                await _webDriverService.GetHtmlWithLoadMoreAsync(url, crawlDataRequestDto.Options.Loader["loader"]);
            TourNodes = await _webContentExtractionService.ExtractNodesByXpath(htmlFullLoaded, parentXpath);
        }
        else if ((crawlDataRequestDto.Options != null &&
                  crawlDataRequestDto.Options.Pagination != null &&
                  crawlDataRequestDto.Options.Pagination.Count > 0))
        {
            Dictionary<string, string> paginationUrls = crawlDataRequestDto.Options.Pagination;
            foreach (var paginationUrl in paginationUrls)
            {
                var content = await _webContentExtractionService.GetBodyContentByUrl(paginationUrl.Value, true);
                if (content != null)
                {
                    var nodes = await _webContentExtractionService.ExtractNodesByXpath(content, parentXpath);
                    TourNodes.AddRange(nodes);
                }
            }
        }
        else
        {
            string htmlNotOption = await _webContentExtractionService.GetBodyContentByUrl(url, false);
            TourNodes = await _webContentExtractionService.ExtractNodesByXpath(htmlNotOption, parentXpath);
        }


        ValidationHelper.ValidateNotNullOrEmpty(parentXpath, nameof(crawlDataRequestDto.Elements),
            "Parent XPath is required.");

        List<Dictionary<string, object>> results = await CrawlBasicData(TourNodes, crawlDataRequestDto, url);
        ValidationHelper.CheckEmptyBeforeInsert(results);
        return results;
    }

    
    public async Task<List<Dictionary<string, object>>> CrawlBasicData(List<HtmlNode> TourNodes,
        CrawlDataRequestDto crawlDataRequestDto, string url)
    {
        var results = new List<Dictionary<string, object>>();

        foreach (var tourCard in TourNodes)
        {
            var tourData = new Dictionary<string, object>();

            foreach (var element in crawlDataRequestDto.Elements.Where(e =>
                         !e.Key.Equals("Parent", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var fieldName = element.Key;
                    var xpath = element.Value;

                    switch (fieldName.ToLower())
                    {
                        case "tourcode":
                            var codeNode = tourCard.SelectSingleNode(xpath);

                            var codeText = codeNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(codeText) ? string.Empty : codeText;


                            break;
                        case "departurelocation":
                            var locationNode = tourCard.SelectSingleNode(xpath);
                            var locationText = locationNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(locationText) ? string.Empty : locationText;


                            break;

                        case "itinerary":
                            var itineraryNode = tourCard.SelectSingleNode(xpath);
                            var itineraryText = itineraryNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(itineraryText) ? string.Empty : itineraryText;

                            break;

                        case "duration":
                            var durationNode = tourCard.SelectSingleNode(xpath);

                            var durationText = durationNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(durationText) ? string.Empty : durationText;

                            break;

                        case "departureTime":
                            var departureTimeNode = tourCard.SelectSingleNode(xpath);
                            var departureTimeText = departureTimeNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(departureTimeText)
                                ? string.Empty
                                : departureTimeText;

                            break;

                        case "transportation":
                            var transportNode = tourCard.SelectSingleNode(xpath);

                            var transportText = transportNode.InnerText?.Trim();
                            tourData[fieldName] =
                                string.IsNullOrEmpty(transportText) ? string.Empty : transportText;


                            break;

                        case "departuredates":
                            string dateXpath = xpath;
                            bool extractText = xpath.EndsWith("/text()");
                            if (extractText)
                            {
                                dateXpath = xpath.Replace("/text()", "");
                            }

                            var dateNodes = tourCard.SelectNodes(dateXpath);

                            var dates = dateNodes.Select(node => node.InnerText?.Trim())
                                .Where(text => !string.IsNullOrEmpty(text)).ToList();
                            tourData[fieldName] = dates;

                            break;

                        case "title":
                            var titleNode = tourCard.SelectSingleNode(xpath);

                            var titleText = titleNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(titleText) ? string.Empty : titleText;


                            break;
                        
                        case "detailslink":
                            if (crawlDataRequestDto.Detail?.Basic?.Elements?.Count > 0 &&
                                crawlDataRequestDto.Detail?._Program?.Elements?.Count > 0)
                            {
                                var linkNode = tourCard.SelectSingleNode(xpath);
                                var hrefValue = linkNode.GetAttributeValue("href", string.Empty)?.Trim();
                                var hrefFinal = _utilsService.CheckAndAppendUrl(hrefValue, url);
                                if (string.IsNullOrEmpty(hrefValue))
                                {
                                    throw new InvalidOperationException("Href value cannot be null or empty.");
                                }

                                await CrawlBasicDetailData(hrefFinal, crawlDataRequestDto.Detail, tourData);
                            }

                            break;

                        case "thumbnail":
                            var imgNode = tourCard.SelectSingleNode(xpath);

                            string? imgSrc = imgNode.GetAttributeValue("data-src", null)?.Trim() ??
                                             imgNode.GetAttributeValue("data-original", null)?.Trim() ??
                                             imgNode.GetAttributeValue("data-lazy", null)?.Trim() ??
                                             imgNode.GetAttributeValue("data-ll-src", null)?.Trim() ??
                                             imgNode.GetAttributeValue("data-srcset", null)?.Trim() ??
                                             imgNode.GetAttributeValue("srcset", null)?.Trim() ??
                                             imgNode.GetAttributeValue("src", null)?.Trim();
                            if (!string.IsNullOrEmpty(imgSrc))
                            {
                                if (!imgSrc.StartsWith("http://") && !imgSrc.StartsWith("https://"))
                                {
                                    var uri = new Uri(crawlDataRequestDto.Url);
                                    var baseUrl = $"{uri.Scheme}://{uri.Host}";

                                    imgSrc = imgSrc.StartsWith("/") ? baseUrl + imgSrc : baseUrl + "/" + imgSrc;
                                }

                                tourData[fieldName] = imgSrc;
                            }
                            else
                            {
                                tourData[fieldName] = string.Empty;
                            }

                            break;

                        case "price":
                            var priceNode = tourCard.SelectSingleNode(xpath);
                            var priceText = priceNode.InnerText?.Trim();
                            tourData[fieldName] = priceText?.Replace("&nbsp;", " ").Trim() ?? string.Empty;
                            break;

                        case "priceold":
                            var priceOldNode = tourCard.SelectSingleNode(xpath);

                            var priceOldText = priceOldNode.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(priceOldText) &&
                                !priceOldText.Equals("Giá từ:", StringComparison.OrdinalIgnoreCase) &&
                                (priceOldText.Contains('₫') || priceOldText.Contains("VND") ||
                                 priceOldText.Contains('$')))
                            {
                                tourData[fieldName] = priceOldText.Replace("&nbsp;", " ").Trim();
                            }
                            else
                            {
                                tourData[fieldName] = String.Empty;
                            }


                            break;
                        default:
                            var node = tourCard.SelectSingleNode(xpath);

                            var text = node.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(text) ? string.Empty : text;

                            break;
                    }
                }
                catch (Exception)
                {
                    tourData[element.Key] = string.Empty;
                }
            }

            results.Add(tourData);
        }

        return results;
    }


    public async Task CrawlBasicDetailData(string urlDetail, _Detail detail, Dictionary<string, object> tourData)
    {
        ValidationHelper.ValidateNotNullOrEmpty(urlDetail, nameof(urlDetail), "URL detail cannot be null or empty.");
        ValidationHelper.ValidateNotNull(detail, nameof(detail), "Detail cannot be null.");
        ValidationHelper.ValidateNotNull(tourData, nameof(tourData), "Tour data cannot be null.");

        string html = await _webContentExtractionService.GetBodyContentByUrl(urlDetail, true);
        HtmlDocument bodyContent = new HtmlDocument();
        bodyContent.LoadHtml(html);


        if (detail.Basic != null)
        {
            if (detail.Basic.Elements == null)
                ValidationHelper.ValidateNotNull(detail.Basic.Elements, nameof(detail.Basic.Elements),
                    "Basic elements cannot be null.");

            var parentXpath = detail.Basic.Elements
                ?.FirstOrDefault(kvp => kvp.Key.Equals("Parent", StringComparison.OrdinalIgnoreCase)).Value;

            ValidationHelper.ValidateNotNullOrEmpty(parentXpath, nameof(detail.Basic.Elements),
                "Parent XPath is required for basic detail data extraction.");

            var basicContent = bodyContent.DocumentNode.SelectSingleNode(parentXpath);

            foreach (var element in detail.Basic.Elements.Where(e =>
                         !e.Key.Equals("Parent", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var fieldName = element.Key;
                    var xpath = element.Value;

                    switch (fieldName.ToLower())
                    {
                        case "tourcode":
                            var codeNode = basicContent.SelectSingleNode(xpath);

                            var codeText = codeNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(codeText) ? string.Empty : codeText;

                            break;
                        case "departurelocation":
                            var locationNode = basicContent.SelectSingleNode(xpath);

                            var locationText = locationNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(locationText) ? string.Empty : locationText;

                            break;
                        case "duration":
                            var durationNode = basicContent.SelectSingleNode(xpath);

                            var durationText = durationNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(durationText) ? string.Empty : durationText;

                            break;
                        case "transportation":
                            var transportNode = basicContent.SelectSingleNode(xpath);

                            var transportText = transportNode.InnerText?.Trim();
                            tourData[fieldName] =
                                string.IsNullOrEmpty(transportText) ? string.Empty : transportText;

                            break;

                        case "departuretime":
                            var departureTimeNode = basicContent.SelectSingleNode(xpath);
                            var departureTimeText = departureTimeNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(departureTimeText)
                                ? string.Empty
                                : departureTimeText;

                            break;

                        case "departuredates":
                            string dateXpath = xpath;
                            bool extractText = xpath.EndsWith("/text()");
                            if (extractText)
                            {
                                dateXpath = xpath.Replace("/text()", "");
                            }

                            var dateNodes = basicContent.SelectNodes(dateXpath);

                            var dates = dateNodes.Select(node => node.InnerText?.Trim())
                                .Where(text => !string.IsNullOrEmpty(text)).ToList();
                            tourData[fieldName] = dates;

                            break;

                        case "title":
                            var titleNode = basicContent.SelectSingleNode(xpath);

                            var titleText = titleNode.InnerText?.Trim();
                            tourData[fieldName] = string.IsNullOrEmpty(titleText) ? string.Empty : titleText;

                            break;

                        case "price":
                            var priceNode = basicContent.SelectSingleNode(xpath);
                            var priceText = priceNode.InnerText?.Trim();
                            tourData[fieldName] = priceText?.Replace("&nbsp;", " ").Trim() ?? string.Empty;
                            break;

                        case "priceold":
                            var priceOldNode = basicContent.SelectSingleNode(xpath);

                            var priceOldText = priceOldNode.InnerText?.Trim();
                            if (!string.IsNullOrEmpty(priceOldText) &&
                                !priceOldText.Equals("Giá từ:", StringComparison.OrdinalIgnoreCase) &&
                                (priceOldText.Contains('₫') || priceOldText.Contains("VND") ||
                                 priceOldText.Contains('$')))
                            {
                                tourData[fieldName] = priceOldText.Replace("&nbsp;", " ").Trim();
                            }
                            else
                            {
                                tourData[fieldName] = String.Empty;
                            }


                            break;

                        default:
                            var node = basicContent.SelectSingleNode(xpath);
                            if (node != null)
                            {
                                var text = node.InnerText?.Trim();
                                tourData[fieldName] = string.IsNullOrEmpty(text) ? string.Empty : text;
                            }
                            else
                            {
                                tourData[fieldName] = string.Empty;
                            }

                            break;
                    }
                }
                catch (Exception)
                {
                    tourData[element.Key] = string.Empty;
                }
            }

            await CrawlProgramData(urlDetail, bodyContent, detail._Program, tourData);
        }
    }


    public async Task CrawlProgramData(string urlDetail, HtmlDocument bodyContent, __Program program,
        Dictionary<string, object> tourData)
    {
        string html = await _webContentExtractionService.GetHtmlAsync(urlDetail);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        foreach (var element in program.Elements)
        {
            try
            {
                var fieldName = element.Key;
                var xpath = element.Value;

                switch (fieldName.ToLower())
                {
                    case "images":
                        var parentNode = doc.DocumentNode.SelectSingleNode(xpath);
                        var imageUrls = new List<string>();

                        if (parentNode != null)
                        {
                            var imageNodes = parentNode.SelectNodes(".//img");
                            if (imageNodes != null)
                            {
                                foreach (var node in imageNodes)
                                {
                                    string? imgSrc = node.GetAttributeValue("data-src", null)?.Trim() ??
                                                     node.GetAttributeValue("data-original", null)?.Trim() ??
                                                     node.GetAttributeValue("data-lazy", null)?.Trim() ??
                                                     node.GetAttributeValue("data-ll-src", null)?.Trim() ??
                                                     node.GetAttributeValue("data-srcset", null)?.Trim() ??
                                                     node.GetAttributeValue("srcset", null)?.Trim() ??
                                                     node.GetAttributeValue("src", null)?.Trim();

                                    if (!string.IsNullOrEmpty(imgSrc))
                                    {
                                        if (!imgSrc.StartsWith("http://") && !imgSrc.StartsWith("https://"))
                                        {
                                            var uri = new Uri(urlDetail);
                                            var baseUrl = $"{uri.Scheme}://{uri.Host}";
                                            imgSrc = imgSrc.StartsWith("/") ? baseUrl + imgSrc : baseUrl + "/" + imgSrc;
                                        }

                                        imageUrls.Add(imgSrc);
                                    }
                                }
                            }
                        }

                        tourData[fieldName] = imageUrls;

                        break;

                    case "popupTriggerXpath":
                        string htmlOriginal = await _webContentExtractionService.GetHtmlAsync(urlDetail);
                        string htmlDisplayedPopup =
                            await _webDriverService.GetHtmlWithPopupAsync(urlDetail, xpath);

                        string popupXpath =
                            await _webContentExtractionService.GetPopupXpath(htmlOriginal, htmlDisplayedPopup);

                        var popupNode = doc.DocumentNode.SelectSingleNode(popupXpath);
                        var _imageUrls = new List<string>();

                        if (popupNode != null)
                        {
                            var imageNodes = popupNode.SelectNodes(".//img");
                            if (imageNodes != null)
                            {
                                foreach (var node in imageNodes)
                                {
                                    string? imgSrc = node.GetAttributeValue("data-src", null)?.Trim() ??
                                                     node.GetAttributeValue("data-original", null)?.Trim() ??
                                                     node.GetAttributeValue("data-lazy", null)?.Trim() ??
                                                     node.GetAttributeValue("data-ll-src", null)?.Trim() ??
                                                     node.GetAttributeValue("data-srcset", null)?.Trim() ??
                                                     node.GetAttributeValue("srcset", null)?.Trim() ??
                                                     node.GetAttributeValue("src", null)?.Trim();

                                    if (!string.IsNullOrEmpty(imgSrc))
                                    {
                                        if (!imgSrc.StartsWith("http://") && !imgSrc.StartsWith("https://"))
                                        {
                                            var uri = new Uri(urlDetail);
                                            var baseUrl = $"{uri.Scheme}://{uri.Host}";
                                            imgSrc = imgSrc.StartsWith("/") ? baseUrl + imgSrc : baseUrl + "/" + imgSrc;
                                        }

                                        _imageUrls.Add(imgSrc);
                                    }
                                }
                            }
                        }

                        tourData[fieldName] = _imageUrls;

                        break;


                    case "highlights":
                        var highlightNode = doc.DocumentNode.SelectNodes(xpath);

                        var highlights = highlightNode.Select(node => node.InnerText?.Trim())
                            .Where(text => !string.IsNullOrEmpty(text))
                            .Select(text => text.Replace("&amp", "").Trim())
                            .Where(text => !string.IsNullOrEmpty(text))
                            .ToList();
                        tourData[fieldName] = highlights;

                        break;

                    case "schedule":
                        var scheduleNode = doc.DocumentNode.SelectNodes(xpath);
                        var schedules = scheduleNode
                            .Select(node => string.Join(" ", node.ChildNodes
                                    .Where(n => n.NodeType == HtmlNodeType.Text || n.NodeType == HtmlNodeType.Element)
                                    .Select(n => n.InnerText.Trim()))
                                .Replace(":-/-/", "")
                                .Replace("&nbsp;", "")
                                .Replace("S/T/T", "")
                                .Replace("S/T/-T", "")
                                .Replace("ĂN:-/-/", "")
                                .Replace("ĂN:S/T/T", "")
                                .Replace("ĂN:S/T/-T", "")
                                .Trim())
                            .Where(text => !string.IsNullOrEmpty(text))
                            .ToList();
                        tourData[fieldName] = schedules;
                        break;

                    default:
                        var _node = doc.DocumentNode.SelectSingleNode(xpath);

                        var text = _node.InnerText?.Trim();
                        tourData[fieldName] = string.IsNullOrEmpty(text) ? string.Empty : text;


                        break;
                }
            }
            catch (Exception)
            {
                tourData[element.Key] = string.Empty;
            }
        }
    }
}