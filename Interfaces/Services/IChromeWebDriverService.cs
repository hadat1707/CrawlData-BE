using OpenQA.Selenium.Chrome;

namespace CrawlProject.Interfaces.Services;

public interface IChromeWebDriverService
{
    Task<string> GetHtmlStealthAsync(string url);
    Task<string> GetHtmlNonHeadlessAsync(string url);
    Task<string> GetHtmlMinimalAsync(string url);
    Task<string> GetHtmlFastAsync(string url);
    Task<string> GetHtmlWithLoadMoreAsync(string url, string loadMoreButtonXPath);
    Task<string> GetHtmlWithPopupAsync(string url, string popupXpath);
}