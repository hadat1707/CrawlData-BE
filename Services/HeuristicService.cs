using HtmlAgilityPack;
using CrawlProject.Interfaces.Services;
using CrawlProject.Core.Constants;
using CrawlProject.Utils;

namespace CrawlProject.Services;

public class HeuristicService : IHeuristicService
{
    private readonly IUtilsService _utilsService;
    private readonly List<DataFieldDefinition> _fieldDefinitions;
    private readonly Lazy<IWebContentExtractionService> _webContentExtractionService;

    public HeuristicService(IUtilsService utilsService, Lazy<IWebContentExtractionService> webContentExtractionService)
    {
        _utilsService = utilsService ?? throw new ArgumentNullException(nameof(utilsService));
        _fieldDefinitions = DataFieldTourDefinition.FieldDefinitions;
        _webContentExtractionService = webContentExtractionService;
    }
    
  public async Task<string> ExistsElementByElementDefinitions(List<string> urlDetails)
{
    string contentForTwoElements = string.Empty;
    string contentForOneElement = string.Empty;
    
    foreach (var url in urlDetails)
    {
        var bodyContent = await _webContentExtractionService.Value.GetBodyContentByUrl(url, true);
        
        var (hasSchedule, hasHighlight, hasImages) = FindPresentElementsInHtml(bodyContent);
        
        if (hasSchedule && hasHighlight && hasImages)
        {
            return bodyContent;
        }
        
        if (string.IsNullOrEmpty(contentForTwoElements))
        {
            bool hasTwoElements = (hasSchedule && hasHighlight) || (hasSchedule && hasImages) || (hasHighlight && hasImages);
            if (hasTwoElements)
            {
                contentForTwoElements = bodyContent;
            }
        }
        if (string.IsNullOrEmpty(contentForOneElement))
        {
            if (hasSchedule || hasHighlight || hasImages)
            {
                contentForOneElement = bodyContent;
            }
        }
    }


    if (!string.IsNullOrEmpty(contentForTwoElements))
    {
        return contentForTwoElements;
    }

    if (!string.IsNullOrEmpty(contentForOneElement))
    {
        return contentForOneElement;
    }


    return string.Empty;
}
  
private (bool hasSchedule, bool hasHighlight, bool hasImages) FindPresentElementsInHtml(string htmlContent)
{
    var doc = new HtmlDocument();
    doc.LoadHtml(htmlContent);

    bool foundSchedule = false;
    bool foundHighlight = false;
    bool foundImages = false;

    // Iterate through all nodes on the page.
    foreach (var node in doc.DocumentNode.Descendants())
    {
        foreach (var definition in DataFieldTourDefinition.ElementDefinitions)
        {
            if (definition.ScoringFunction(node) > 0)
            {
                switch (definition.FieldName)
                {
                    case "schedule":
                        foundSchedule = true;
                        break;
                    case "highLight":
                        foundHighlight = true;
                        break;
                    case "images":
                        foundImages = true;
                        break;
                }
            }
        }

        // Optimization: If all elements have been found, stop scanning the rest of the page.
        if (foundSchedule && foundHighlight && foundImages)
        {
            break;
        }
    }

    return (foundSchedule, foundHighlight, foundImages);
}


    public List<HtmlNode> findChildBlocks(HtmlDocument doc, string parent_Xpath)
    {
        var xPathDynamic = _utilsService.getXpathDyanamic(parent_Xpath);
        var parentNode = doc.DocumentNode.SelectSingleNode(xPathDynamic);
        if (parentNode is not null)
        {
            return parentNode.ChildNodes
                .Where(ch => ch.NodeType == HtmlNodeType.Element)
                .ToList();
        }

        return new List<HtmlNode>();
    }

    public List<Dictionary<string, object>> ScrapeDynamicData(string htmlRaw, HtmlDocument doc, string parent_Xpath,
        string type)
    {
        var results = new List<Dictionary<string, object>>();
        var tourBlocks = new List<HtmlNode>();

        HtmlDocument workingDoc;
        if (htmlRaw != null)
        {
            workingDoc = new HtmlDocument();
            workingDoc.LoadHtml(htmlRaw);
        }
        else if (doc != null)
        {
            workingDoc = doc;
        }
        else
        {
            throw new ArgumentNullException("Both htmlRaw and doc cannot be null.");
        }

        if (parent_Xpath != null)
        {
            tourBlocks = findChildBlocks(workingDoc, parent_Xpath);
        }
        else
        {
            var bodyNode = workingDoc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                tourBlocks = bodyNode.ChildNodes
                    .Where(ch => ch.NodeType == HtmlNodeType.Element)
                    .ToList();
            }
            else
            {
                tourBlocks = workingDoc.DocumentNode.ChildNodes
                    .Where(ch => ch.NodeType == HtmlNodeType.Element)
                    .ToList();
            }
        }

        foreach (var block in tourBlocks)
        {
            var scrapedData = ProcessBlock(block, type);
            if (scrapedData.Count > 0)
            {
                results.Add(scrapedData);
            }
        }

        return results;
    }

    private Dictionary<string, object> ProcessBlock(HtmlNode block, string type)
    {
        var allNodes = block.Descendants().ToList();
        var bestCandidates = new Dictionary<string, (HtmlNode Node, int Score)>();
        if (type == "link")
        {
            foreach (var node in allNodes)
            {
                foreach (var definition in _fieldDefinitions)
                {
                    int score = definition.ScoringFunction(node);
                    if (score > 0 && (!bestCandidates.ContainsKey(definition.FieldName) ||
                                      score > bestCandidates[definition.FieldName].Score))
                    {
                        bestCandidates[definition.FieldName] = (node, score);
                    }
                }
            }

            var scrapedData = new Dictionary<string, object>();
            foreach (var candidate in bestCandidates)
            {
                string fieldName = candidate.Key;
                HtmlNode bestNode = candidate.Value.Node;
                string value;
                if (fieldName == "Link" && bestNode.Name == "a")
                {
                    value = bestNode.GetAttributeValue("href", "").Trim();
                }
                else
                {
                    value = bestNode.InnerText.Trim();
                }

                scrapedData[fieldName] = value;

                return scrapedData;
            }

            return new Dictionary<string, object>();
        }
        else
        {
            throw new ArgumentException("Invalid type specified. Use 'link' or 'label'.");
        }
    }
}