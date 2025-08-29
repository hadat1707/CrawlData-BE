namespace CrawlProject.Models;

public class XpathModel
{
    public string Website { get; set; } = string.Empty;
    public Dictionary<string, string>? Basic { get; set; }

    public _OptionsModel? Options { get; set; }
    public _DetailModel? Detail { get; set; }
}

public class _OptionsModel
{
    public Dictionary<string, string>? Pagination { get; set; }
    public Dictionary<string, string>? Loader { get; set; }
}

public class _DetailModel
{
    public _BasicModel? Basic { get; set; }
    public __ProgramModel? _Program { get; set; }
}

public class _BasicModel
{
    public Dictionary<string, string>? Elements { get; set; }
}

public class __ProgramModel
{
    public Dictionary<string, string>? Elements { get; set; }
}