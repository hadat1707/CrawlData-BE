using CrawlProject.Interfaces.Services;
using MongoDB.Driver;
using CrawlProject.Dto;
using System.Threading.Tasks;
using CrawlProject.Models;

namespace CrawlProject.Services;

public class MongoDatabaseService : IMongoDatabaseService
{
    private readonly IMongoDatabase _database;

    public MongoDatabaseService(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<bool> checkBasicDataExists(string key, string url)
    {
        var collection = _database.GetCollection<XpathModel>("Xpaths");
        var filter = Builders<XpathModel>.Filter.And(
            Builders<XpathModel>.Filter.Eq("Website", url),
            Builders<XpathModel>.Filter.Eq("Basic." + key, true)
        );

        var result = await collection.Find(filter).FirstOrDefaultAsync();
        return result != null;
    }

    public async Task<bool> checkPaginationExists(string key, string url)
    {
        var collection = _database.GetCollection<XpathModel>("Xpaths");
        var filter = Builders<XpathModel>.Filter.And(
            Builders<XpathModel>.Filter.Eq("Website", url),
            Builders<XpathModel>.Filter.Eq("Options.Pagination." + key, true)
        );

        var result = await collection.Find(filter).FirstOrDefaultAsync();
        return result != null;
    }


    public async Task InsertCrawlDataAsync(XpathModel crawlData)
    {
        var collection = _database.GetCollection<XpathModel>("Xpaths");
        await collection.InsertOneAsync(crawlData);
    }
}