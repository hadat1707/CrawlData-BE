namespace CrawlProject.Core.Constants;

public static class CrawlConstants
{
    public static class ContentTypes
    {
        public const string Link = "link";
        public const string Image = "image";
        public const string Text = "text";
    }
    
    public static class LogMessages
    {
        public const string StartingAnalysis = "Starting HTML analysis for URL: {Url}";
        public const string AnalyzingWithAI = "Analyzing content with OpenAI for URL: {Url}";
        public const string AIAnalysisFailed = "OpenAI analysis failed for URL: {Url}, continuing without selector";
        public const string ExtractingByXPath = "Extracting HTML by XPath: {xpath}";
        public const string XPathExtractionFailed = "Failed to get HTML by xPath {Selector} for URL: {Url}, using full HTML as fallback";
    }

    public static class ValidationMessages
    {
        public const string UrlRequired = "URL is required.";
        public const string InvalidUrlFormat = "Invalid URL format.";
    }
}
