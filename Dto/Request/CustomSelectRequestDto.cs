using System.Text.Json.Serialization;

namespace CrawlProject.Dto;

public class CustomSelectRequestDto
{
    //danh cho list
    public string? url  { get; set; }
    //danh cho detail
    public string? urlDetail { get; set; }
    // danh cho list
    public string? parentListXpath { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PageType pageType { get; set; }

    public string request { get; set; }
}

public enum PageType
{
    Details,
    List
}