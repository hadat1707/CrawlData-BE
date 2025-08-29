namespace CrawlProject.Core.Configuration;

public class ChromeDriverConfiguration
{
    public const string SectionName = "ChromeDriver";
    public int TimeoutSeconds { get; set; } = 30;
    public bool Headless { get; set; } = true;
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    public List<string> Arguments { get; set; } = new()
    {
        "--disable-blink-features=AutomationControlled",
        "--disable-extensions",
        "--no-sandbox",
        "--disable-dev-shm-usage"
    };
}
