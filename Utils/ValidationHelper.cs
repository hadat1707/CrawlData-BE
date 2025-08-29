using System.Collections;
using CrawlProject.Dto;

namespace CrawlProject.Utils;

public static class ValidationHelper
{
    public static void ValidateNotNull<T>(T value, string paramName, string message = null) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName, message ?? $"{paramName} cannot be null");
    }

    public static void ValidateNotNullOrEmpty(string value, string paramName, string message = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(paramName, message ?? $"{paramName} cannot be null or empty");
    }
    
    public static void CheckEmptyBeforeInsert(List<Dictionary<string, object>> results)
    {
        if (results == null || !results.Any())
        {
            return;
        }

        var candidateKeys = results.First().Keys.ToList();
        var keysToRemove = new HashSet<string>();

        foreach (var key in candidateKeys)
        {
            bool isUniversallyEmpty = results.All(tour =>
            {
                if (!tour.TryGetValue(key, out var value))
                {
                    return true;
                }

                return value switch
                {
                    string stringValue => string.IsNullOrEmpty(stringValue),
                    ICollection collectionValue => collectionValue.Count == 0,
                    null => true,
                    _ => false
                };
            });

            if (isUniversallyEmpty)
            {
                keysToRemove.Add(key);
            }
        }

        if (keysToRemove.Any())
        {
            foreach (var tour in results)
            {
                foreach (var key in keysToRemove)
                {
                    tour.Remove(key);
                }
            }
        }
    }
}