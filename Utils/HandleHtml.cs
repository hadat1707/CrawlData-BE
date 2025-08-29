using System.Text.RegularExpressions;
using CrawlProject.Interfaces.Services;
using HtmlAgilityPack;

namespace CrawlProject.Utils;

public class HandleHtml
{
    private readonly IGeminiService _geminiService;

    public HandleHtml(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public string RemoveSpace(string html)
    {
        string htmlRemoved = Regex.Replace(html, @"[\t\n\r]+", "");
        return htmlRemoved;
    }

    public string CleanHtmlTourDetail(string html)
    {
        html = Regex.Replace(html, "<!--.*?-->", string.Empty, RegexOptions.Singleline);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes(
            "//svg|//script|//style|//iframe|//footer|//nav|//aside|//input|//button|//select|//option|//textarea|//i|//noscript|//false");
        if (nodes != null)
        {
            foreach (var node in nodes.ToList())
                node.Remove();
        }

        var extraNodes = doc.DocumentNode
            .Descendants()
            .Where(n => n.NodeType == HtmlNodeType.Element &&
                        (
                            (n.GetAttributeValue("class", "").ToLower().Contains("menu")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("pagination")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("search")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("breadcrumb")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("advertisement")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobi")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobile")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("login")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("register")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("register")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("contact")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("social")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("share")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("icon")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("footer")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("footer")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("zalo")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("hotline")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("logo")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("news")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("blog")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("rating")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("widget")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("widget")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("alert")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("alert")) ||
                            (
                                (n.GetAttributeValue("class", "").ToLower().Contains("popup") ||
                                 n.GetAttributeValue("id", "").ToLower().Contains("popup")) &&
                                n.SelectNodes(".//img")?.Count == 0
                            ) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("calendar")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("calendar")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("table")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("table")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("review")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("review")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("note")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("note")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("service")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("service")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("tourRelated")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("tourRelated")) ||
                            ((n.GetAttributeValue("id", "").ToLower().Contains("hide") ||
                              n.GetAttributeValue("class", "").ToLower().Contains("hide")) &&
                             n.SelectNodes(".//img")?.Count == 0)
                        ))
            .ToList();

        foreach (var node in extraNodes)
            node.Remove();

        foreach (var node in doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
        {
            foreach (var attr in node.Attributes
                         .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                                     a.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                         .ToList())
                node.Attributes.Remove(attr);
        }

        return doc.DocumentNode.OuterHtml;
    }

    public string CleanHtmlInsideParentXpath(string html)
    {
        html = Regex.Replace(html, "<!--.*?-->", string.Empty, RegexOptions.Singleline);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes(
            "//svg|//script|//style|//iframe|//nav|//aside|//input|//button|//select|//option|//textarea|//i");
        if (nodes != null)
        {
            foreach (var node in nodes.ToList())
                node.Remove();
        }

        var extraNodes = doc.DocumentNode
            .Descendants()
            .Where(n => n.NodeType == HtmlNodeType.Element &&
                        (
                            (n.GetAttributeValue("class", "").ToLower().Contains("search")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("breadcrumb")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("advertisement")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobi")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobile")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("archive"))
                        ))
            .ToList();

        foreach (var node in extraNodes)
            node.Remove();

        foreach (var node in doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
        {
            foreach (var attr in node.Attributes.Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                         .ToList())
                node.Attributes.Remove(attr);
        }

        foreach (var node in doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
        {
            foreach (var attr in node.Attributes
                         .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                                     a.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                         .ToList())
                node.Attributes.Remove(attr);
        }

        return doc.DocumentNode.OuterHtml;
    }

    public async Task<string> CleanDynamic(string html)
    {
        List<string> PeripheralXpath = await _geminiService.GetPeripheralWrappersXpathListAsync(html);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (string xpath in PeripheralXpath)
        {
            var node = doc.DocumentNode.SelectSingleNode(xpath);
            if (node != null)
            {
                node.Remove();
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    public HtmlDocument CleanHtml(HtmlNode html)
    {
        var doc = new HtmlDocument();
        string htmlContent = Regex.Replace(html.OuterHtml, "<!--.*?-->", string.Empty, RegexOptions.Singleline);
        doc.LoadHtml(htmlContent);

        var nodes = doc.DocumentNode.SelectNodes(
            "//svg|//script|//style|//iframe|//footer|//aside|//input|//button|//select|//option|//textarea|//noscript");
        if (nodes != null)
        {
            foreach (var node in nodes.ToList())
                node.Remove();
        }

        var navNodes = doc.DocumentNode.SelectNodes("//nav");
        if (navNodes != null)
        {
            foreach (var navNode in navNodes.ToList())
            {
                var classAttr = navNode.GetAttributeValue("class", "").ToLower();
                if (classAttr.Contains("pagination") || classAttr.Contains("paging") || classAttr.Contains("page"))
                {
                    navNode.Remove();
                }
            }
        }

        var extraNodes = doc.DocumentNode
            .Descendants()
            .Where(n => n.NodeType == HtmlNodeType.Element &&
                        (
                            (n.GetAttributeValue("id", "").ToLower().Equals("header") ||
                             n.GetAttributeValue("id", "").ToLower().Equals("footer")) ||
                            (n.GetAttributeValue("class", "").ToLower().Equals("header") ||
                             n.GetAttributeValue("class", "").ToLower().Equals("footer")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("breadcrumb")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("advertisement")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobi")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("mobile")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("archive")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("popup")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("popup")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("news")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("news")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("note")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("note")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("service")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("service")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("owl")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("owl")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("menu")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("menu")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("sidebar") &&
                             n.Descendants().Any(child =>
                                 child.GetAttributeValue("class", "").ToLower().Contains("panel") ||
                                 child.GetAttributeValue("class", "").ToLower().Contains("filter"))) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("sidebar") &&
                             n.Descendants().Any(child =>
                                 child.GetAttributeValue("class", "").ToLower().Contains("panel") ||
                                 child.GetAttributeValue("class", "").ToLower().Contains("filter"))) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("modal")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("modal")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("search-form")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("search-form")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("media")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("media")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("popover")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("popover")) ||
                            (n.GetAttributeValue("class", "").ToLower().Contains("navbar")) ||
                            (n.GetAttributeValue("id", "").ToLower().Contains("navbar"))
                        ))
            .ToList();

        foreach (var node in extraNodes)
            node.Remove();

        foreach (var node in doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
        {
            foreach (var attr in node.Attributes
                         .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                                     a.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                         .ToList())
                node.Attributes.Remove(attr);
        }

        return doc;
    }
}