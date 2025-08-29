using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CrawlProject.Utils;

public class DataFieldDefinition
{
    public string FieldName { get; }
    public Func<HtmlNode, int> ScoringFunction { get; }

    public DataFieldDefinition(string fieldName, Func<HtmlNode, int> scoringFunction)
    {
        FieldName = fieldName;
        ScoringFunction = scoringFunction;
    }
}

public class DataFieldTourDefinition
{
    public static List<DataFieldDefinition> ElementDefinitions { get; } = new List<DataFieldDefinition>
    {
        new DataFieldDefinition("schedule", node =>
        {
            int score = 0;
            string[] listTag = { "div", "span", "p", "h1", "h2", "h3", "h4", "h5", "h6" };
            if (listTag.Contains(node.Name))
            {
                score += 10;
            }

            if (node.GetClasses().Any(c => c.Contains("title")))
            {
                score += 20;
            }

            if (!string.IsNullOrWhiteSpace(node.InnerText) &&
                node.InnerText.IndexOf("Lịch trình", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score += 20;
            }

            return score;
        }),
        new DataFieldDefinition("highLight", node =>
        {
            int score = 0;
            string[] listTag = { "div", "span", "p", "h1", "h2", "h3", "h4", "h5", "h6" };
            if (listTag.Contains(node.Name))
            {
                score += 10;
            }

            if (node.GetClasses().Any(c => c.Contains("title")))
            {
                score += 20;
            }

            if (!string.IsNullOrWhiteSpace(node.InnerText) &&
                (node.InnerText.IndexOf("Điểm nhấn hành trình", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 node.InnerText.IndexOf("Điểm nhấn của chương trình", StringComparison.OrdinalIgnoreCase) >= 0) || 
                 node.InnerText.IndexOf("Trải nghiệm thú vị trong tour", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                score += 20;
            }

            return score;
        }),
        new DataFieldDefinition("images",node =>
        {
            int score = 0;
            
         
            if (node.Name == "img")
            {
             
                if (!string.IsNullOrEmpty(node.GetAttributeValue("src", "")))
                {
                    score += 30; 
                }
        
                if (!string.IsNullOrEmpty(node.GetAttributeValue("alt", "")))
                {
                    score += 10;
                }
        
                if (node.GetAttributeValue("loading", "") == "lazy")
                {
                    score += 5;
                }
        
                if (!string.IsNullOrEmpty(node.GetAttributeValue("srcset", "")))
                {
                    score += 8; 
                }
        
                var imgClasses = node.GetClasses();
                
                if (imgClasses.Any(c => c.Contains("thumbnail") || c.Contains("featured") || c.Contains("main")))
                {
                    score += 15; 
                }
        

                if (imgClasses.Any(c => c.Contains("slide") || c.Contains("carousel") || c.Contains("swiper")))
                {
                    score += 20; 
                }
        
   
                var parent = node.ParentNode;
                var depth = 0;
                while (parent != null && depth < 5) 
                {
                    var parentClasses = parent.GetClasses();
                    
                    if (parentClasses.Any(c => 
                        c.Contains("carousel", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("slider", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("swiper", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("owl-carousel", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("slick", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 25; 
                        break;
                    }
                    
                    if (parentClasses.Any(c => 
                        c.Contains("gallery", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("lightbox", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("fancybox", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 20;
                        break;
                    }
                    
                    if (parentClasses.Any(c => 
                        c.Contains("image-grid", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("img-list", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("photo-grid", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 18; 
                        break;
                    }
        
                    parent = parent.ParentNode;
                    depth++;
                }
        
                var tourImageKeywords = new[] { "tour", "destination", "travel", "trip", "attraction", "du lịch", "điểm đến" };
                var altText = node.GetAttributeValue("alt", "").ToLower();
                var titleText = node.GetAttributeValue("title", "").ToLower();
                
                if (tourImageKeywords.Any(keyword => 
                    altText.Contains(keyword) || titleText.Contains(keyword)))
                {
                    score += 15; 
                }
            }
            
            var imgElements = node.Descendants("img").ToList();
            if (imgElements.Any() && node.Name != "img")
            {
                int imageCount = imgElements.Count;
                var containerClasses = node.GetClasses();
                
               
                if (imageCount == 1)
                {
                    score += 10; 
                    
                    if (containerClasses.Any(c => 
                        c.Contains("image-wrapper", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("img-container", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("photo-container", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 8; 
                    }
                }
                
                else if (imageCount > 1)
                {
                    score += 15;
                    
                    if (containerClasses.Any(c => 
                        c.Contains("carousel", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("slider", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("swiper", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 30; 
                        
                        if (containerClasses.Any(c => 
                            c.Contains("owl-carousel", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("slick", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("swiper-container", StringComparison.OrdinalIgnoreCase) ||
                            c.Contains("glider", StringComparison.OrdinalIgnoreCase)))
                        {
                            score += 10; 
                        }
                    }
                    
                    if (containerClasses.Any(c => 
                        c.Contains("gallery", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("lightbox", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("photo-gallery", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 25; 
                    }
                    
                    if (containerClasses.Any(c => 
                        c.Contains("grid", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("masonry", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("isotope", StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 20; 
                    }
                    
                    if (imageCount >= 5)
                    {
                        score += 10; 
                    }
                    if (imageCount >= 10)
                    {
                        score += 5; 
                    }
                }
                
                var hasNavigation = node.Descendants().Any(d => 
                    d.GetClasses().Any(c => 
                        c.Contains("nav", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("arrow", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("prev", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("next", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("dot", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("indicator", StringComparison.OrdinalIgnoreCase)));
                
                if (hasNavigation)
                {
                    score += 15; 
                }
                
                var dataAttributes = node.Attributes.Where(a => a.Name.StartsWith("data-"));
                if (dataAttributes.Any(attr => 
                    attr.Name.Contains("slide", StringComparison.OrdinalIgnoreCase) ||
                    attr.Name.Contains("carousel", StringComparison.OrdinalIgnoreCase) ||
                    attr.Name.Contains("swiper", StringComparison.OrdinalIgnoreCase) ||
                    attr.Name.Contains("gallery", StringComparison.OrdinalIgnoreCase)))
                {
                    score += 12; 
                }
            }
            
            return Math.Max(0, score);
        }),
    };
    
    public static List<DataFieldDefinition> FieldDefinitions { get; } = new List<DataFieldDefinition>
    {
        new DataFieldDefinition("Link", node =>
        {
            if (node.Name != "a") return 0;
            int score = 0;

            var href = node.GetAttributeValue("href", "");
            if (!string.IsNullOrEmpty(href))
            {
                score += 30;

                if (Uri.TryCreate(href, UriKind.Absolute, out var uri))
                {
                    score += 10; 

                    if (uri.Scheme == "https")
                    {
                        score += 5;
                    }
                }
                else if (href.StartsWith("/") || href.StartsWith("./") || href.StartsWith("../"))
                {
                    score += 5; 
                }

                if (href.StartsWith("javascript:") || href.StartsWith("mailto:") || href.StartsWith("tel:"))
                {
                    score -= 15;
                }
            }

            var linkText = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(linkText))
            {
                if (linkText.Length > 5)
                {
                    score += 10;
                }

                if (linkText.Length > 15)
                {
                    score += 5;
                }

                var genericTexts = new[] { "click here", "read more", "more", "link", "here", "xem thêm", "chi tiết" };
                if (genericTexts.Any(gt => linkText.Equals(gt, StringComparison.OrdinalIgnoreCase)))
                {
                    score -= 5;
                }

                var tourKeywords = new[]
                    { "tour", "travel", "trip", "booking", "đặt tour", "chi tiết tour", "xem tour" };
                if (tourKeywords.Any(tk => linkText.Contains(tk, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 15;
                }
            }

            if (!string.IsNullOrEmpty(href))
            {
                var detailsPatterns = new[] { "detail", "details", "view", "show", "info", "chi-tiet", "thong-tin" };
                if (detailsPatterns.Any(pattern => href.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 20; 
                }
            }

            if (!string.IsNullOrEmpty(node.GetAttributeValue("title", "")))
            {
                score += 5;

                var titleValue = node.GetAttributeValue("title", "");
                var tourKeywords = new[]
                {
                    "tour", "travel", "trip", "booking", "đặt tour", "chi tiết tour", "xem tour", "details", "chi tiết"
                };
                if (tourKeywords.Any(tk => titleValue.Contains(tk, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 10;
                }
            }

            var linkClasses = node.GetClasses();
            if (linkClasses.Any(c => c.Contains("btn") || c.Contains("button")))
            {
                score += 10;
            }

            if (linkClasses.Any(c => c.Contains("detail") || c.Contains("more") || c.Contains("view")))
            {
                score += 8;
            }

            if (linkClasses.Any(c => c.Contains("primary") || c.Contains("main")))
            {
                score += 6;
            }

            var parent = node.ParentNode;
            var tourContextKeywords = new[] { "tour", "travel", "trip", "package", "booking", "destination" };

            while (parent != null && parent.Name != "body")
            {
                var parentClasses = parent.GetClasses();
                if (parentClasses.Any(c =>
                        tourContextKeywords.Any(tk => c.Contains(tk, StringComparison.OrdinalIgnoreCase))))
                {
                    score += 12;
                    break;
                }

                var parentText = parent.GetDirectInnerText();
                if (!string.IsNullOrEmpty(parentText) &&
                    tourContextKeywords.Any(tk => parentText.Contains(tk, StringComparison.OrdinalIgnoreCase)))
                {
                    score += 8;
                    break;
                }

                parent = parent.ParentNode;
            }

            if (node.Descendants("img").Any())
            {
                score += 8; 
            }

            var dataAttributes = node.Attributes.Where(a => a.Name.StartsWith("data-"));
            if (dataAttributes.Any())
            {
                score += 3; 
            }

            if (!string.IsNullOrEmpty(href) && Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
            {
                var suspiciousDomains = new[] { "ads.", "ad.", "doubleclick", "googleads", "facebook.com/tr" };
                if (suspiciousDomains.Any(sd => absoluteUri.Host.Contains(sd)))
                {
                    score -= 20;
                }
            }

            return Math.Max(0, score);
        }),
    };
}