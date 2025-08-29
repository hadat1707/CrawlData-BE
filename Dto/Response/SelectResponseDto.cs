namespace CrawlProject.Dto.Response;

public class SelectResponseDto
{
    public Dictionary<string, string>? Elements { get; set; }
    public Options? Options { get; set; }
    public Detail? Detail { get; set; }
}

public class Options
{
    public Dictionary<string, string>? Pagination { get; set; }
    public Dictionary<string, string>? Loader { get; set; }
}

public class Detail
{
    public Basic? Basic { get; set; }
    public _Program? _Program { get; set; }
}

public class Basic
{
    public Dictionary<string, string>? Elements { get; set; }
}

public class _Program
{
    public Dictionary<string, string>? Elements { get; set; }
}