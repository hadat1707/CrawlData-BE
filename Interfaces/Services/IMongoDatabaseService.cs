using CrawlProject.Dto;
using System.Threading.Tasks;
using CrawlProject.Models;

namespace CrawlProject.Interfaces.Services;

public interface IMongoDatabaseService
{
    Task InsertCrawlDataAsync(XpathModel crawlData);
}