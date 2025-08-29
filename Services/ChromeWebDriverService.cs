using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using CrawlProject.Interfaces.Services;

namespace CrawlProject.Services;

public class ChromeWebDriverService : IChromeWebDriverService
{
    private readonly ILogger<ChromeWebDriverService> _logger;

    public ChromeWebDriverService(ILogger<ChromeWebDriverService> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetHtmlStealthAsync(string url)
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(
            "--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);

        using var driver = new ChromeDriver(options);

        try
        {
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);

            _logger.LogInformation("Getting HTML from URL (stealth): {Url}", url);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(75));
            await Task.Run(() => driver.Navigate().GoToUrl(url), cts.Token);

            await Task.Delay(5000, cts.Token);

            var html = driver.PageSource;

            _logger.LogInformation("Successfully retrieved HTML (stealth): {Url}", url);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in stealth mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new TimeoutException($"WebDriver timeout in stealth mode for URL: {url}", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning("Stealth mode timed out for URL: {Url}", url);
            throw new TimeoutException($"Stealth mode timed out for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in stealth mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in stealth mode for URL: {url}", ex);
        }
    }

    public async Task<string> GetHtmlNonHeadlessAsync(string url)
    {
        var options = new ChromeOptions();

        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(
            "--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        using var driver = new ChromeDriver(options);

        try
        {
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(40);

            _logger.LogInformation("Getting HTML from URL (non-headless): {Url}", url);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(55));
            await Task.Run(() => driver.Navigate().GoToUrl(url), cts.Token);
            await Task.Delay(5000, cts.Token);

            var html = driver.PageSource;

            _logger.LogInformation("Successfully retrieved HTML (non-headless): {Url}", url);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in non-headless mode for URL: {Url}, Error: {Error}", url,
                ex.Message);
            throw new TimeoutException($"WebDriver timeout in non-headless mode for URL: {url}", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning("Non-headless mode timed out for URL: {Url}", url);
            throw new TimeoutException($"Non-headless mode timed out for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in non-headless mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in non-headless mode for URL: {url}", ex);
        }
    }

    public async Task<string> GetHtmlMinimalAsync(string url)
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-javascript");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-css");

        using var driver = new ChromeDriver(options);

        try
        {
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(25);

            _logger.LogInformation("Getting HTML from URL (minimal): {Url}", url);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            await Task.Run(() => driver.Navigate().GoToUrl(url), cts.Token);

            var html = driver.PageSource;

            _logger.LogInformation("Successfully retrieved HTML (minimal): {Url}", url);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in minimal mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new TimeoutException($"WebDriver timeout in minimal mode for URL: {url}", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning("Minimal mode timed out for URL: {Url}", url);
            throw new TimeoutException($"Minimal mode timed out for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in minimal mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in minimal mode for URL: {url}", ex);
        }
    }


    public async Task<string> GetHtmlFastAsync(string url)
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--window-size=1280,720");
        options.AddArgument(
            "--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        using var driver = new ChromeDriver(options);

        try
        {
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            _logger.LogInformation("Getting HTML from URL (fast): {Url}", url);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Task.Run(() => driver.Navigate().GoToUrl(url), cts.Token);

            await Task.Delay(2000, cts.Token);

            var html = driver.PageSource;

            if (string.IsNullOrEmpty(html) || html.Length < 500)
            {
                throw new Exception("HTML content appears to be empty or too short");
            }

            _logger.LogInformation("Successfully retrieved HTML (fast): {Url}, length: {Length}", url, html.Length);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in fast mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new TimeoutException($"WebDriver timeout in fast mode for URL: {url}", ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning("Fast mode timed out for URL: {Url}", url);
            throw new TimeoutException($"Fast mode timed out for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in fast mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in fast mode for URL: {url}", ex);
        }
    }

    public async Task<string> GetHtmlWithPopupAsync(string url, string popupXpath)
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(
            "--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        using var driver = new ChromeDriver(options);

        try
        {
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            _logger.LogInformation("Getting HTML with popup handling from URL: {Url}", url);

            driver.Navigate().GoToUrl(url);

            await Task.Delay(3000);

            try
            {
                var popupButton = driver.FindElement(By.XPath(popupXpath));

                if (popupButton.Displayed)
                {
                    _logger.LogInformation("Found popup button, clicking...");

                    driver.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});",
                        popupButton);
                    await Task.Delay(1000);

                    try
                    {
                        popupButton.Click();
                    }
                    catch (ElementClickInterceptedException)
                    {
                        driver.ExecuteScript("arguments[0].click();", popupButton);
                    }

                    await Task.Delay(3000);
                }
                else
                {
                    _logger.LogInformation("Popup button is not displayed, continuing without interaction");
                }
            }
            catch (NoSuchElementException)
            {
                _logger.LogInformation("Popup button not found, continuing without interaction");
            }
            catch (StaleElementReferenceException)
            {
                _logger.LogInformation("Popup button became stale, trying to find it again...");
                await Task.Delay(1000);
            }

            var html = driver.PageSource;
            _logger.LogInformation("Successfully retrieved HTML with popup: {Url}, length: {Length}", url, html.Length);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in load more mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new TimeoutException($"WebDriver timeout in load more mode for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in load more mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in load more mode for URL: {url}", ex);
        }
    }


    public async Task<string> GetHtmlWithLoadMoreAsync(string url, string loadMoreButtonXPath)
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument(
            "--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        using var driver = new ChromeDriver(options);

        try
        {
            driver.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            _logger.LogInformation("Getting HTML with load more functionality from URL: {Url}", url);

            driver.Navigate().GoToUrl(url);

            await Task.Delay(3000);

            string previousPageSource = driver.PageSource;
            int consecutiveNoChangeClicks = 0;
            const int maxNoChangeClicks = 3;

            while (true)
            {
                try
                {
                    var loadMoreButton = driver.FindElement(By.XPath(loadMoreButtonXPath));

                    if (!loadMoreButton.Displayed)
                    {
                        _logger.LogInformation("Load more button is not displayed, stopping");
                        break;
                    }

                    _logger.LogInformation("Found load more button, clicking...");

                    driver.ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});",
                        loadMoreButton);
                    await Task.Delay(1000);

                    try
                    {
                        loadMoreButton.Click();
                    }
                    catch (ElementClickInterceptedException)
                    {
                        driver.ExecuteScript("arguments[0].click();", loadMoreButton);
                    }

                    await Task.Delay(3000);

                    string currentPageSource = driver.PageSource;
                    if (currentPageSource.Length == previousPageSource.Length ||
                        Math.Abs(currentPageSource.Length - previousPageSource.Length) < 100)
                    {
                        consecutiveNoChangeClicks++;
                        _logger.LogInformation("No significant content change detected after click #{ClickCount} " +
                                               "(consecutive no-change clicks: {NoChangeClicks}/{MaxNoChangeClicks})",
                            consecutiveNoChangeClicks, consecutiveNoChangeClicks, maxNoChangeClicks);

                        if (consecutiveNoChangeClicks >= maxNoChangeClicks)
                        {
                            _logger.LogInformation("No content changes after {MaxNoChangeClicks} consecutive clicks, " +
                                                   "assuming all data is loaded", maxNoChangeClicks);
                            break;
                        }
                    }
                    else
                    {
                        consecutiveNoChangeClicks = 0;
                        _logger.LogInformation("Content changed after click #{ClickCount}, continuing...",
                            consecutiveNoChangeClicks + 1);
                    }

                    previousPageSource = currentPageSource;
                }
                catch (NoSuchElementException)
                {
                    _logger.LogInformation("Load more button not found, finished loading all content");
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    _logger.LogInformation("Load more button became stale, trying to find it again...");
                    await Task.Delay(1000);
                }
            }

            await Task.Delay(2000);

            var html = driver.PageSource;

            if (string.IsNullOrEmpty(html) || html.Length < 500)
            {
                throw new Exception("HTML content appears to be empty or too short");
            }

            _logger.LogInformation("Successfully retrieved HTML with load more: {Url}, length: {Length}",
                url, html.Length);
            return html;
        }
        catch (WebDriverTimeoutException ex)
        {
            _logger.LogWarning("WebDriver timeout in load more mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new TimeoutException($"WebDriver timeout in load more mode for URL: {url}", ex);
        }
        catch (WebDriverException ex)
        {
            _logger.LogError("WebDriver error in load more mode for URL: {Url}, Error: {Error}", url, ex.Message);
            throw new Exception($"WebDriver error in load more mode for URL: {url}", ex);
        }
    }
}