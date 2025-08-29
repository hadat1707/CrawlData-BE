using Core.Configuration;
using CrawlProject.Core.Configuration;
using CrawlProject.Interfaces.Services;
using CrawlProject.Services;
using CrawlProject.Utils;

namespace CrawlProject.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrawlServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiConfiguration>(
            configuration.GetSection(GeminiConfiguration.SectionName));
        services.Configure<ChromeDriverConfiguration>(
            configuration.GetSection(ChromeDriverConfiguration.SectionName));
        services.Configure<MongoDbConfiguration>(
            configuration.GetSection("MongoDb"));
        
        services.AddScoped<IWebContentExtractionService, WebContentExtractionService>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IHeuristicService, HeuristicService>();
        services.AddScoped<IUtilsService, UtilsService>();
        services.AddScoped<IChromeWebDriverService, ChromeWebDriverService>();
        services.AddScoped<ICrawlService, CrawlService>();
        services.AddScoped<ILinkDiscoveryService, LinkDiscoveryService>();
        services.AddScoped<IExcelService, ExcelService>();
        
        services.AddScoped(provider => new Lazy<IHeuristicService>(() => provider.GetRequiredService<IHeuristicService>()));
        services.AddScoped(provider => new Lazy<IWebContentExtractionService>(() => provider.GetRequiredService<IWebContentExtractionService>()));
        
        services.AddScoped<HandleHtml>();
        
        var mongoConfig = configuration.GetSection("MongoDb").Get<MongoDbConfiguration>();
        var mongoClient = new MongoDB.Driver.MongoClient(mongoConfig.ConnectionString);
        services.AddSingleton(mongoClient);
        services.AddSingleton(sp => mongoClient.GetDatabase(mongoConfig.DatabaseName));
        services.AddScoped<IMongoDatabaseService, MongoDatabaseService>();

        return services;
    }

    public static IServiceCollection AddHttpClientServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        return services;
    }
}
