namespace CrawlProject.Core.Configuration;

public class GeminiConfiguration
{
    public const string SectionName = "Gemini";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.5-pro";
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
    public int MaxTokens { get; set; } = 110000;
}
