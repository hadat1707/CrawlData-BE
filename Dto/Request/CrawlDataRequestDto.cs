namespace CrawlProject.Dto;

public class CrawlDataRequestDto
{
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Elements { get; set; }
    
    public _Options? Options { get; set; }
    public _Detail? Detail { get; set; }
    
}

public class _Options
{
    public Dictionary<string, string>? Pagination { get; set; }
    public Dictionary<string, string>? Loader { get; set; }
}

public class _Detail
{
    public _Basic? Basic { get; set; }
    public __Program? _Program { get; set; }
}

public class _Basic
{
    public Dictionary<string, string>? Elements { get; set; }
}

public class __Program
{
    public Dictionary<string, string>? Elements { get; set; }
}