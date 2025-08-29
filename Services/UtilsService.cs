using System.Text.RegularExpressions;
using CrawlProject.Dto;
using CrawlProject.Dto.Response;
using CrawlProject.Interfaces.Services;
using CrawlProject.Utils;

namespace CrawlProject.Services;

public class UtilsService : IUtilsService
{
    public void CleanElement(SelectResponseDto selectResponseDto)
    {
        ValidationHelper.ValidateNotNull(selectResponseDto, nameof(selectResponseDto),
            "SelectResponseDto cannot be null");

        if (selectResponseDto.Elements == null || selectResponseDto.Elements.Count == 0)
            throw new ArgumentException("Elements cannot be null or empty");

        if (selectResponseDto.Elements.Count == 1 && selectResponseDto.Elements.ContainsKey("parent"))
        {
            selectResponseDto.Elements = null;
        }
        else if (selectResponseDto.Detail.Basic.Elements.Count == 1 &&
                 selectResponseDto.Detail.Basic.Elements.ContainsKey("parent"))
        {
            selectResponseDto.Detail.Basic = null;
        }
        else if (selectResponseDto.Detail._Program.Elements.Count == 0)
        {
            selectResponseDto.Detail._Program = null;
        }
    }

    public void CompareElement(Dictionary<string, string> basic, Dictionary<string, string> detail)
    {
        ValidationHelper.ValidateNotNull(basic, nameof(basic), "Basic element cannot be null");
        ValidationHelper.ValidateNotNull(detail, nameof(detail), "Detail element cannot be null");

        if (basic.Count == 0 || detail.Count == 0)
            throw new ArgumentException("Both basic and detail elements must contain at least one key-value pair.");

        RemoveEmptyValues(basic);
        RemoveEmptyValues(detail);

        var basicKeysLower = basic.Keys
            .Where(key => !key.Contains("parent", StringComparison.OrdinalIgnoreCase))
            .Select(key => key.ToLowerInvariant())
            .ToHashSet();

        var keysToRemoveFromDetail = detail.Keys
            .Where(key => basicKeysLower.Contains(key.ToLowerInvariant()))
            .ToList();

        foreach (var key in keysToRemoveFromDetail)
        {
            detail.Remove(key);
        }
    }

    private static void RemoveEmptyValues(Dictionary<string, string> dictionary)
    {
        var keysToRemove = dictionary
            .Where(kvp => string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }
    
    public string CheckAndAppendUrl(string url, string urlOrigin)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url), "Url cannot be null");

        if (string.IsNullOrWhiteSpace(urlOrigin))
            throw new ArgumentNullException(nameof(urlOrigin), "Url origin cannot be null or empty");

        var baseUrl = GetBaseUrl(urlOrigin);

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("Base URL cannot be determined from the provided URL origin.");

        var newUrl = string.Empty;
        if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        {
            if (url.StartsWith("/"))
            {
                newUrl = baseUrl + url;
            }
            else
            {
                newUrl = new Uri(baseUrl + url).ToString();
            }
        }

        return !string.IsNullOrWhiteSpace(newUrl) ? newUrl : url;
    }

    public List<string> HandleUrl(string urlOrigin, List<Dictionary<string, object>> urlTourDetails)
    {
        List<string> UrlDetails = new List<string>();
        var baseUrl = GetBaseUrl(urlOrigin);

        if (urlTourDetails != null)
        {
            foreach (var detail in urlTourDetails)
            {
                if (detail.ContainsKey("Link"))
                {
                    var link = detail["Link"] as string;
                    if (!string.IsNullOrWhiteSpace(link) &&
                        !link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (link.StartsWith("/"))
                            detail["Link"] = baseUrl + link;
                        else
                            detail["Link"] = baseUrl + "/" + link;
                        link = detail["Link"] as string;
                    }

                    if (!string.IsNullOrWhiteSpace(link))
                        UrlDetails.Add(link);
                }
            }
        }

        return UrlDetails;
    }

    public string GetBaseUrl(string urlOrigin)
    {
        if (string.IsNullOrEmpty(urlOrigin))
            throw new ArgumentNullException(nameof(urlOrigin));

        var uri = new Uri(urlOrigin);
        string baseUrl = $"{uri.Scheme}://{uri.Host}";
        if (!uri.IsDefaultPort)
            baseUrl += $":{uri.Port}";

        return baseUrl;
    }

    public string getXpathDyanamic(string xPath)
    {
        if (xPath == null)
            throw new ArgumentNullException(nameof(xPath), "XPath cannot be null");

        string tag = string.Empty;

        var parts = xPath.Split(new[] { "//" }, StringSplitOptions.None);
        if (parts.Length > 1)
        {
            var tagPart = parts[1].Split('[')[0].Split('/')[0];
            tag = tagPart;
        }

        if (xPath.Contains("[@"))
        {
            var attrMatch = Regex.Match(xPath, @"\[@(class|id)='([^']+)'\]");
            if (attrMatch.Success)
            {
                var attrName = attrMatch.Groups[1].Value;
                var attrValue = attrMatch.Groups[2].Value;

                if (!string.IsNullOrEmpty(tag) && !string.IsNullOrEmpty(attrValue))
                {
                    return BuildContainsXPath(tag, attrName, attrValue);
                }
            }
        }

        throw new ArgumentException("Invalid XPath format. Expected format: //tag[@attribute='value']");
    }

    private string BuildContainsXPath(string tag, string attribute, string value)
    {
        if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(value))
            throw new ArgumentException("Tag, attribute, and value must be provided");

        return $"//{tag}[contains(concat(' ', normalize-space(@{attribute}), ' '), ' {value} ')]";
    }
}