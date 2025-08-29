using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GenerativeAI;
using System.Linq;
using CrawlProject.Interfaces.Services;
using CrawlProject.Core.Configuration;
using CrawlProject.Core.Constants;

namespace CrawlProject.Services;

public class GeminiService : IGeminiService
{
    private readonly GenerativeModel _geminiClient;
    private readonly ILogger<GeminiService> _logger;
    private readonly GeminiConfiguration _configuration;

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _configuration = configuration.GetSection(GeminiConfiguration.SectionName).Get<GeminiConfiguration>()
                         ?? throw new InvalidOperationException("Gemini configuration not found");

        var apiKey = _configuration.ApiKey;
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Gemini API key not found in configuration");

        _geminiClient = new GenerativeModel(apiKey, _configuration.Model);
        _logger = logger;
    }

    public async Task<List<string>> GetPeripheralWrappersXpathListAsync(string html)
    {
        return await RetryWithBackoffAsync<List<string>>(async () =>
        {
            var fullPrompt = $@"
You are a master-level AI specializing in advanced DOM structural analysis and precision XPath generation. Your mission is to analyze the provided HTML document and generate ultra-robust, absolute XPaths for the primary structural and navigational parent elements of web pages. You excel at identifying major layout components (""chrome"") through sophisticated semantic analysis, accessibility patterns, and content validation.

**ADVANCED ANALYSIS FRAMEWORK:**

### **SEMANTIC INTELLIGENCE PRIORITY MATRIX:**
1. **HTML5 SEMANTIC SUPREMACY**: Prioritize modern semantic elements (`<header>`, `<footer>`, `<nav>`, `<aside>`, `<main>`, `<section>`) with proper context validation
2. **ARIA LANDMARK MASTERY**: Leverage ARIA roles (`banner`, `contentinfo`, `navigation`, `complementary`, `search`) and labels for accessibility-driven identification
3. **MICRODATA AWARENESS**: Recognize Schema.org structured data and microformat patterns
4. **FUNCTIONAL PATTERN RECOGNITION**: Analyze element purpose through content patterns, interaction models, and layout behavior
5. **HIERARCHICAL CONTEXT VALIDATION**: Ensure elements are appropriately positioned within the document structure
6. **CROSS-BROWSER COMPATIBILITY**: Generate XPaths that work across different rendering engines and viewport sizes

### **ENHANCED TARGET IDENTIFICATION METHODOLOGY:**

#### **1) HEADER - Primary Site Banner**
**Semantic Priorities:**
- `<header>` element at document root level with site-wide navigation and branding
- `div[@role=""banner""]` with appropriate landmark characteristics
- Container with site logo, primary navigation, search, and user account elements

**Content Validation Matrix:**
- ✅ **MUST CONTAIN**: Logo/brand element, primary navigation menu
- ✅ **SHOULD CONTAIN**: Search functionality, user account/login links, language selectors
- ✅ **POSITION VALIDATION**: Upper 20% of viewport, not nested within `<main>` or `<article>`
- ❌ **EXCLUSIONS**: Article headers, modal headers, component-specific headers

#### **2) FOOTER - Primary Site Information Hub**
**Semantic Priorities:**
- `<footer>` element at document root with site-wide information and links
- `div[@role=""contentinfo""]` containing copyright, legal, and company information
- Bottom-positioned container with structured site-wide links and metadata

**Content Validation Matrix:**
- ✅ **MUST CONTAIN**: Copyright notice, contact information, or legal links
- ✅ **SHOULD CONTAIN**: Company details, social media links, sitemap navigation
- ✅ **POSITION VALIDATION**: Lower 30% of document, follows main content chronologically
- ❌ **EXCLUSIONS**: Article footers, component footers, inline citation footers

**Vietnamese Language Patterns:**
- Text patterns: ""Bản quyền"", ""Liên hệ"", ""Điều khoản"", ""Chính sách"", ""Công ty""

#### **3) BREADCRUMB - Hierarchical Navigation**
**Semantic Priorities:**
- `<nav>` with `aria-label=""Breadcrumb""` or breadcrumb-specific labeling
- `<ol>` or `<ul>` with sequential hierarchical links showing page ancestry
- Navigation element positioned between header and main content


**Structure Validation Patterns:**
- ✅ **HIERARCHY INDICATORS**: Separators (>, /, », →, |), ordered progression
- ✅ **LINK CHAIN**: Multiple clickable elements showing page hierarchy
- ✅ **POSITION LOGIC**: Between site header and main content area
- ❌ **EXCLUSIONS**: Pagination, tabs, general navigation menus

#### **4) FILTER - Content Refinement Interface**
**Semantic Priorities:**
- `<aside>` or `<form>` or `<section>` containing multiple filter categories and controls
- Container with filter-specific headings and interactive elements
- Faceted navigation interface for content refinement

**Functional Validation Requirements:**
- ✅ **MULTIPLE CONTROLS**: 3+ distinct filter categories (checkboxes, selects, ranges)
- ✅ **FILTER SEMANTICS**: Headings like ""Filter"", ""Refine"", ""Bộ lọc"", ""Tìm kiếm nâng cao"", ""bộ lọc tìm kiếm ""
- ✅ **INTERACTIVE ELEMENTS**: Form controls that modify content display
- ❌ **EXCLUSIONS**: Single search boxes, sort controls, simple form inputs

#### **5) SORT BAR - Content Ordering Interface**
**Semantic Priorities:**
- Container with sorting controls and ordering options
- Elements with sort-specific labels and dropdown/button interfaces
- Positioned near content listings for result manipulation

**Control Pattern Recognition:**
- ✅ **SORT CONTROLS**: `<select>` dropdowns, radio buttons, toggle buttons
- ✅ **SORT LABELS**: ""Sort by"", ""Order by"", ""Sắp xếp theo"", ""Xếp theo""
- ✅ **POSITIONING**: Above or adjacent to sortable content areas
- ❌ **EXCLUSIONS**: Filter forms, pagination controls, general navigation

#### **6) SIDEBAR - Complementary Content Area**
**Semantic Priorities:**
- `<aside>` element containing supplementary content and widgets
- Complementary content area positioned alongside main content
- Container for related links, advertisements, or secondary navigation

**Layout and Content Validation:**
- ✅ **COMPLEMENTARY ROLE**: Secondary content that supports but doesn't replace main content
- ✅ **SIDEBAR POSITIONING**: Typically left or right of main content column
- ✅ **WIDGET CONTENT**: Related articles, advertisements, social feeds, navigation aids
- ❌ **EXCLUSIONS**: Main content areas, primary navigation, essential page elements

#### **7) CATEGORY NAVIGATION - Section Switching Interface**
**Semantic Priorities:**
- `<nav>` containing category or section switching controls
- Tab-like interface for major content category navigation
- Horizontal or vertical navigation for content type selection

**Interface Pattern Validation:**
- ✅ **TAB BEHAVIOR**: `role=""tablist""`, active/inactive states, mutual exclusivity
- ✅ **CATEGORY SCOPE**: Major content sections, not page-to-page navigation
- ✅ **SWITCHING INTERFACE**: Clear active states and category boundaries
- ❌ **EXCLUSIONS**: Main site navigation, breadcrumbs, pagination

#### **8) SEARCH BOX - Site-wide Query Interface**
**Semantic Priorities:**
- `<form>` with `role=""search""` containing site-wide search functionality
- Search input with submit mechanism for content discovery
- Prominently positioned search interface for site exploration

**Search Pattern Recognition:**
- ✅ **SEARCH SEMANTICS**: Search input, submit button, search role attributes
- ✅ **SITE-WIDE SCOPE**: General content search, not filtered or scoped searches
- ✅ **PROMINENT PLACEMENT**: Header area, easily accessible location
- ❌ **EXCLUSIONS**: Filter search boxes, scoped search forms, internal searches

#### **9-16) FOOTER SUBSECTIONS - ORGANIZED INFORMATION ARCHITECTURE**

**9) POLICY LINKS** - Legal and Privacy Information
- ✅ Links to: Terms of Service, Privacy Policy, Legal notices, Compliance information
- 🏷️ Vietnamese: ""Chính sách"", ""Điều khoản"", ""Bảo mật"", ""Pháp lý""

**10) SERVICE LINKS** - Business Offerings
- ✅ Links to: Products, Services, Business solutions, Commercial offerings
- 🏷️ Vietnamese: ""Dịch vụ"", ""Sản phẩm"", ""Giải pháp""

**11) NEWS LINKS** - Content and Updates
- ✅ Links to: Blog posts, News articles, Updates, Press releases
- 🏷️ Vietnamese: ""Tin tức"", ""Blog"", ""Cẩm nang"", ""Báo chí""

**12) COMPANY NOTE** - Corporate Information
- ✅ Contains: Company details, Contact information, Business registration, Address
- 🏷️ Vietnamese: ""Công ty"", ""Địa chỉ"", ""Hotline"", ""Email"", ""Đăng ký kinh doanh""

**13) EVENT LISTINGS** - Temporal Content
- ✅ Contains: Event schedules, Upcoming events, Calendar information
- 🏷️ Vietnamese: ""Sự kiện"", ""Lịch sự kiện"", ""Hoạt động""

**14) SOCIAL MEDIA LINKS** - Platform Connections
- ✅ Links to: Facebook, Twitter, Instagram, LinkedIn, YouTube, TikTok
- 🏷️ Recognizable: Social platform domains, social media icons

**15) MAIN NAVIGATION** - Primary Site Structure
- ✅ Contains: Main site sections, Primary page navigation, Core site areas
- 🏷️ Positioned: Header area, main navigation role

**16) CONTACT INFORMATION** - Communication Details
- ✅ Contains: Contact forms, Phone numbers, Email addresses, Physical addresses
- 🏷️ Vietnamese: ""Liên hệ"", ""Điện thoại"", ""Email"", ""Địa chỉ""

---

### **ULTRA-ROBUST XPATH CONSTRUCTION RULES:**

**INTELLIGENT SELECTOR HIERARCHY:**
1. **🎯 SEMANTIC ELEMENTS**: Native HTML5 semantic tags with context validation
2. **🏷️ ARIA LANDMARKS**: Accessibility roles and labels with content verification
3. **🔑 UNIQUE IDENTIFIERS**: Meaningful `@id` attributes with semantic naming
4. **📋 SEMANTIC CLASSES**: Descriptive class names that appear uniquely in the document
5. **📍 CONTEXTUAL POSITIONING**: Structural position combined with content validation
6. **⚡ POSITIONAL FALLBACK**: Pure structural selectors as last resort

**VALIDATION AND ROBUSTNESS STRATEGIES:**
- ✅ **UNIQUENESS GUARANTEE**: Each XPath resolves to exactly one element
- ✅ **CONTENT VERIFICATION**: Selected elements contain expected content types
- ✅ **SEMANTIC PREFERENCE**: Favor meaningful markup over generic containers
- ✅ **ACCESSIBILITY COMPLIANCE**: Leverage ARIA and semantic HTML patterns
- ✅ **CROSS-DEVICE COMPATIBILITY**: XPaths work across desktop, tablet, mobile layouts
- ✅ **FUTURE-PROOF DESIGN**: Resilient to minor HTML structure changes

**CONTENT VALIDATION ALGORITHMS:**
When multiple elements match structural criteria, prioritize based on:
1. **Content Richness Score**: Quantity and quality of expected content types
2. **Semantic Markup Quality**: Proper use of HTML5 elements and ARIA attributes  
3. **Positional Appropriateness**: Logical placement within document flow
4. **Accessibility Compliance**: Support for screen readers and assistive technologies
5. **Cross-Browser Consistency**: Reliable identification across different browsers

**INTELLIGENT OUTPUT OPTIMIZATION:**
- Return **EXACTLY 16 items** in the specified order: [header, footer, breadcrumb, filter, sortBar, sidebar, categoryNav, searchBox, policy, service, news, note, event, social, navbar, contact]
- Use `null` for elements that cannot be reliably and uniquely identified
- Ensure each XPath is **absolute**, **unambiguous**, and **production-ready**
- Optimize for **maintainability** and **performance** in automated systems

**QUALITY ASSURANCE CHECKLIST:**
✅ Semantic HTML5 elements prioritized
✅ ARIA landmarks properly leveraged
✅ Content validation implemented
✅ Vietnamese language patterns included
✅ Uniqueness guaranteed
✅ Cross-device compatibility considered
✅ Future-proof structure applied

Your response must be **ONLY the JSON array** of 16 XPath expressions. No explanations, markdown, or additional text.

HTML Document:
{html}
";
            var response = await _geminiClient.GenerateContentAsync(fullPrompt);
            var result = response.Text?.Trim() ?? "";

            if (result.StartsWith("```"))
            {
                var firstNewline = result.IndexOf('\n');
                if (firstNewline > 0) result = result.Substring(firstNewline + 1);
                if (result.EndsWith("```")) result = result.Substring(0, result.Length - 3);
                result = result.Trim();
            }

            _logger.LogInformation("Peripheral wrappers XPath array: {Json}", result);

            List<string> selectors;
            try
            {
                selectors = JsonConvert.DeserializeObject<List<string>>(result) ?? new List<string>();
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response as JSON array: {Response}", result);
                selectors = new List<string>();
            }

            selectors = selectors.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return selectors;
        });
    }


    public async Task<Dictionary<string, string>> GetOptionXpath(string html)
    {
        return await RetryWithBackoffAsync<Dictionary<string, string>>(async () =>
        {
            var fullPrompt = $@"
You are a master-level XPath generation specialist with advanced expertise in pagination and content loading navigation patterns. Your mission is to analyze complex HTML structures and generate ultra-precise, production-ready XPaths for critical navigation elements that control content discovery and user experience flow.

**ADVANCED ANALYSIS METHODOLOGY:**

### **INTELLIGENT PATTERN RECOGNITION FRAMEWORK:**
1. **SEMANTIC NAVIGATION ANALYSIS**: Prioritize elements with proper ARIA roles, semantic markup, and accessibility attributes
2. **FUNCTIONAL BEHAVIOR VALIDATION**: Ensure targeted elements are actually interactive and serve their intended navigation purpose
3. **CONTENT RELATIONSHIP MAPPING**: Analyze spatial and logical relationships between navigation elements and content areas
4. **CROSS-DEVICE COMPATIBILITY**: Generate XPaths that work across responsive layouts and different viewport sizes
5. **PROGRESSIVE ENHANCEMENT AWARENESS**: Account for JavaScript-enhanced navigation patterns and dynamic content loading

### **TARGET IDENTIFICATION MASTERY:**

#### **🎯 PAGINATION CONTAINER - Content Navigation Hub**

**Definition & Purpose:**
The primary wrapper element that orchestrates page-based content navigation, serving as the control center for user-driven content discovery through sequential page access.

**Semantic Priority Hierarchy:**
1. **ARIA-Enhanced Navigation**: `<nav role=""navigation"" aria-label=""pagination"">` or similar accessibility-compliant structures
2. **Semantic HTML5 Elements**: `<nav>` elements specifically containing pagination controls with appropriate labeling
3. **Structured List Containers**: `<ul>` or `<ol>` elements containing sequential page navigation items
4. **Functional Containers**: `<div>` elements with pagination-specific classes and appropriate child navigation elements

**Advanced Identification Strategies:**
- **Content Pattern Analysis**: Look for numbered sequences (1, 2, 3...), directional indicators (Previous, Next, ←, →)
- **ARIA Landmark Detection**: Elements with `aria-label` containing ""pagination"", ""page navigation"", ""page selector""
- **Class Semantic Recognition**: Classes like `pagination`, `pager`, `page-nav`, `page-numbers`, `nav-links`, `page-controls`
- **Structural Validation**: Must contain multiple clickable child elements representing page options

**Content Validation Matrix:**
- ✅ **MUST CONTAIN**: Multiple navigation elements (page numbers, prev/next buttons, or page indicators)
- ✅ **SHOULD CONTAIN**: Clear visual hierarchy and logical page progression indicators
- ✅ **FUNCTIONAL REQUIREMENTS**: Direct child elements must be clickable or contain clickable elements
- ❌ **EXCLUSIONS**: Breadcrumb navigation, tab navigation, general site navigation, filter controls

**Multi-Language Support:**
- English: ""pagination"", ""pages"", ""page navigation"", ""previous"", ""next""
- Vietnamese: ""phân trang"", ""trang"", ""điều hướng trang"", ""trước"", ""tiếp theo""
- Universal: Numeric sequences, arrow symbols (←, →, ‹, ›, «, »)

#### **🔄 LOAD MORE BUTTON - Progressive Content Loading**

**Definition & Purpose:**
Interactive elements that trigger additional content loading through AJAX, infinite scroll mechanisms, or progressive content revelation, enhancing user experience through on-demand content discovery.

**Text Pattern Recognition (Multi-Language):**
**English Variations:**
- Exact matches: ""Load more"", ""Load More"", ""LOAD MORE"", ""Show more"", ""View more"" , ""Read more""
- Pattern variations: ""Load 10 more"", ""Show 5 more items"", ""Load additional content""

**Vietnamese Variations:**
- Primary: ""Xem thêm"", ""Xem nhiều hơn"", ""Tải thêm"", ""Hiển thị thêm""
- Secondary: ""Xem thêm nội dung"", ""Tải thêm dữ liệu"", ""Xem tiếp""

**Advanced Element Targeting:**
1. **Interactive Element Priority**: `<button>`, `<a>` with valid href, `<div>` with click handlers
2. **Accessibility Compliance**: Elements with appropriate ARIA attributes and keyboard navigation support
3. **Visual State Validation**: Elements that are visible, not disabled, and positioned logically within content flow
4. **Content Context Analysis**: Positioned at logical content boundaries (end of lists, bottom of content areas)

**Functional Validation Requirements:**
- ✅ **INTERACTIVITY**: Must be clickable elements or containers with click event handlers
- ✅ **VISIBILITY**: Not hidden via CSS (`display:none`, `visibility:hidden`) or ARIA (`aria-hidden=""true""`)
- ✅ **CONTEXTUAL POSITIONING**: Located near or after main content areas, typically at content boundaries
- ✅ **SEMANTIC MEANING**: Text content clearly indicates progressive loading functionality
- ❌ **EXCLUSIONS**: Pagination buttons, navigation elements, form submit buttons unrelated to content loading

### **ULTRA-ROBUST XPATH CONSTRUCTION STRATEGY:**

**Intelligent Selector Hierarchy (Priority Order):**
1. **🎯 UNIQUE SEMANTIC IDENTIFIERS**: `@id` attributes with clear, semantic naming conventions
2. **🏷️ DATA ATTRIBUTES**: `@data-*` attributes specifically related to navigation functionality
3. **🔍 ARIA ENHANCED TARGETING**: `@role` combined with `@aria-*` attributes for accessibility-driven selection
4. **📋 SEMANTIC CLASS TARGETING**: Unique class names that clearly indicate functional purpose
5. **📍 POSITIONAL WITH VALIDATION**: Structural positioning combined with content and functionality validation
6. **⚡ INTELLIGENT FALLBACK**: Multi-criteria selection with content validation as last resort

**Advanced XPath Validation Techniques:**

**For Pagination Containers:**
```xpath
// Semantic with content validation
//nav[@role='navigation'][contains(@aria-label, 'pagination') or contains(@aria-label, 'page')]

// Class-based with child validation
//div[contains(@class, 'pagination')][count(.//a | .//button) >= 2]

// Structural with content pattern validation
//*[contains(@class, 'page') and not(contains(@class, 'content'))][.//a[matches(text(), '\\d+')] or .//button[matches(text(), 'Previous|Next')]]
```

**For Load More Buttons:**
```xpath
// Text-based with case-insensitive matching
//button[normalize-space(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')) = 'load more']

// Multi-language support with functional validation
//*[self::button or self::a][@href or @onclick or @data-*][
  contains(normalize-space(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')), 'xem thêm') or
  contains(normalize-space(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')), 'load more') or
  contains(normalize-space(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')), 'show more')
]
```

**PRODUCTION QUALITY ASSURANCE:**

**Uniqueness Validation:**
- Each XPath must resolve to exactly **ONE unique element** in the document
- Implement validation predicates to ensure selected elements contain expected functionality
- Prefer elements that are logically positioned within the content flow

**Cross-Browser Compatibility:**
- Generate XPaths that work consistently across Chrome, Firefox, Safari, and Edge
- Account for different CSS rendering and JavaScript execution contexts
- Ensure compatibility with both desktop and mobile viewport variations

**Performance Optimization:**
- Avoid overly complex XPath expressions that could impact page evaluation performance
- Prefer direct targeting over deep traversal when possible
- Use efficient selector strategies that minimize DOM traversal overhead

**Error Handling & Edge Cases:**
- Return `null` for elements that cannot be uniquely and reliably identified
- Handle dynamic content loading scenarios where elements may not be immediately present
- Account for single-page applications with dynamically generated navigation elements

**INTELLIGENT OUTPUT OPTIMIZATION:**

**Response Format Requirements:**
- Return precisely formatted JSON with exactly two keys: ""pagination"" and ""loader""
- Each value must be either a valid, absolute XPath string or `null`
- XPaths must be immediately usable in production crawling and automation scenarios
- Ensure backward compatibility with existing crawling infrastructure

**Quality Validation Checklist:**
✅ XPath targets exactly one unique element  
✅ Selected element serves intended navigation purpose  
✅ Element is visible and interactive  
✅ XPath is robust against minor HTML structure changes  
✅ Multi-language text patterns are properly handled  
✅ Accessibility attributes are leveraged when available  
✅ Cross-device compatibility is ensured  

HTML Content for Analysis:
{html}

**Expected Response Format (JSON only, no explanations):**
{{
  ""pagination"": ""<relative_xpath_to_pagination_container_or_null>"",
  ""loader"": ""<relative_xpath_to_load_more_button_or_null>""
}}";

            var response = await _geminiClient.GenerateContentAsync(fullPrompt);
            var result = response.Text;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (result.StartsWith("```"))
                {
                    var firstNewline = result.IndexOf('\n');
                    if (firstNewline > 0)
                    {
                        result = result.Substring(firstNewline + 1);
                    }

                    if (result.EndsWith("```"))
                    {
                        result = result.Substring(0, result.Length - 3);
                    }

                    result = result.Trim();
                }

                _logger.LogInformation("Tour program sections XPath result (no contains): {XPath}", result);
            }

            Dictionary<string, string> selectors;
            try
            {
                selectors = JsonConvert.DeserializeObject<Dictionary<string, string>>(result) ??
                            new Dictionary<string, string>();
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response as JSON: {Response}", result);
                selectors = new Dictionary<string, string>();
            }

            return RemoveNullValues(selectors);
        });
    }

    public async Task<Dictionary<string, string>> GetParentTourProgramXpathAsync(string html)
    {
        return await RetryWithBackoffAsync<Dictionary<string, string>>(async () =>
        {
            var fullPrompt = $@"
You are an expert XPath generator. Analyze the HTML carefully and return only XPaths that use exact matching (no contains function). Return only valid JSON without explanations.

Analyze the provided HTML content and find SEPARATE XPath expressions for each individual tour program section.

Target these specific sections:
- Tour images/gallery/photos section
- Single image section (when gallery not available)
- Tour highlights/features section
- Tour schedule/itinerary/program section

CRITICAL VALIDATION INSTRUCTIONS FOR ALL SECTIONS:
1. ALWAYS validate that your XPath targets sections containing ACTUAL CONTENT, not just section labels or headers
2. If you find a header element (like ""Gallery"", ""Điểm nhấn"", ""Lịch trình""), target the content container, not just the header
3. Sections must contain meaningful content (images, feature lists, schedule items), not just descriptive headers
4. Look for patterns in the actual content to identify content-containing sections
5. Use the exact class names,id and structure from the provided HTML

SPECIFIC SECTION VALIDATION RULES:

**Images Section**: Must contain actual images or image containers, not just ""Gallery"" headers
- Look for: img tags, image containers, sliders, slide,carousels, lightboxes with actual image content
- Validate: Contains img elements or image-related content, not just section titles
- Avoid: Headers like ""Photo Gallery"", ""Images"" without actual image content

**Single Image Section**: Must contain a single primary image when gallery is not available
- Look for: Single img tags, hero images, main tour images, featured images
- Validate: Contains exactly one primary image or main image element
- Priority: Main tour image, hero banner, primary thumbnail, featured image
- Use when: No gallery/slider/carousel is present, but a single prominent image exists
- Target: The actual img element or its immediate container with exact class matching
- Examples: Hero images, main tour photos, primary thumbnails, featured banners
- Avoid: Gallery thumbnails, multiple image containers, decorative images

**SLIDERS/CAROUSELS/LIGHTBOXES COUNT AS IMAGES (MANDATORY)**
- Treat any slider/carousel/lightbox component as the Images Section **if it contains real image content**:
  * Acceptable content includes:
    - `<img>` tags
    - `<picture>`/`<source>` that resolve to an `<img>`
    - Elements with inline `style` that specify `background-image:` (target the element with that exact style attribute if needed)
  * Recognize these components using ONLY exact attribute/class matches from the provided HTML (no `contains()`), e.g.:
    - Exact class names commonly used by libraries (examples only): ""swiper"", ""swiper-container"", ""slick-slider"", ""owl-carousel"", ""carousel"", ""splide"", ""glide"", ""fotorama"" — **but when generating XPath, use the exact class values present in the HTML**
    - Exact attributes such as `role=""region""` with `aria-label=""Gallery""` (or the exact label in the HTML)
    - Exact data attributes such as `data-fancybox=""...""`, `data-gallery=""...""` (use the exact value)
- If multiple slider/gallery containers exist, prefer the one with the **largest number of image items**; if ambiguous, select the **first in DOM order** that contains valid image content.
- If a slider uses an overlay showing remaining items like ""+3"", ""+4"", etc., apply the POPUP IMMEDIATE PARENT PRIORITY LOGIC below.

**PRIORITY: POPUP IMMEDIATE PARENT DETECTION FOR IMAGES**
- **PRIMARY TARGET**: Find the IMMEDIATE PARENT element of any element containing ""+number"" text patterns (like +3, +4, +5, +8, etc.)
- **STEP-BY-STEP PROCESS**:
  1. Search the HTML for elements containing text like ""+3"", ""+4"", ""+5"", ""+8"", ""+more"", ""+ more"", ""more photos"", ""See all"", ""View more""
  2. Once found, identify the IMMEDIATE PARENT (direct parent, one level up) of that text-containing element
  3. Return the XPath targeting that immediate parent element
- **TEXT PATTERNS TO DETECT**:
  * Exact text matches: ""+3"", ""+4"", ""+5"", ""+6"", ""+7"", ""+8"", ""+9"", ""+10""
  * Variations: ""+more"", ""+ more"", ""more photos"", ""See all"", ""View more""
  * Numbers with plus signs indicating remaining items in gallery
- **IMMEDIATE PARENT IDENTIFICATION**:
  * If structure is: <div><span>+4</span></div> → Target the <div> (immediate parent)
  * If structure is: <a><div class=""overlay"">+5</div></a> → Target the <a> (immediate parent)
  * If structure is: <button><p>+3 more</p></button> → Target the <button> (immediate parent)
- **PARENT ELEMENT CHARACTERISTICS**:
  * Usually clickable elements: <a>, <button>, or <div> with onclick handlers
  * Often has classes related to: 'gallery', 'photo', 'image', 'popup', 'trigger', 'more'
  * Contains both images/thumbnails and the overlay with +number text
  * Serves as the clickable trigger for popup/modal functionality
- **VALIDATION REQUIREMENTS**:
  * Verify the parent element actually contains a child with +number text
  * Ensure the parent is the immediate/direct parent, not grandparent or ancestor
  * The parent should be functionally meaningful (clickable, interactive)

**Highlights Section**: Must contain actual feature lists or highlight content, not just """"Điểm nhấn"""" headers
- Look for: Lists of features, bullet points, highlight items, benefit descriptions; ALSO accept sections whose header text is CONTAIN one of the following (after normalize-space()):
  * """"Điểm nhấn""""
  * """"Các điểm nhấn""""
  * """"Điểm nhấn tour""""
  * """"Điểm nhấn hành trình""""
  * """"Điểm nhấn chương trình""""
  * """"Trải nghiệm thú vị trong tour""""
  * """"Điểm nhấn của chương trình""""
  * """"Điểm nổi bật""""
  * """"Diem nhan"""" (ASCII fallback)
  (When such a header is found, target the **content container** that follows or wraps the items, not the header itself.)
- Validate: Contains actual feature text, benefit lists, or highlight descriptions

**Highlights Disambiguation (Class Duplicate)**:
- If the exact @class you would use for the highlights container appears on multiple elements, you **must** disambiguate in this order, using only exact equality checks:
  1) Prefer a unique @id on the correct container if present (e.g., //*[@id=""""tour-highlights""""]).
  2) Anchor to an exact header text (Vietnamese accepted forms above) and select the **nearest following content container** that actually has content:
     - Example pattern (choose the appropriate level present in HTML):
     //*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5][normalize-space() = ""Điểm nhấn"" or
normalize-space() = ""Các điểm nhấn"" or
normalize-space() = ""Điểm nhấn tour"" or
normalize-space() = ""Điểm nhấn hành trình"" or
normalize-space() = ""Điểm nhấn chương trình"" or
normalize-space() = ""Trải nghiệm thú vị trong tour"" or
normalize-space() = ""Điểm nhấn của chương trình"" or
normalize-space() = ""Điểm nổi bật""]/following-sibling::*[1]
  3) If no header anchor is available, combine exact class equality **plus a content constraint** to ensure you target the section with real highlight items:
     - e.g., //div[@class=""""EXACT-CLASS""""][.//*[@class=""""highlight-item""""]
  4) If multiple candidates still remain, choose the element with the **largest count** of `.//li` or `.//*[@class=""""highlight-item""""]`; if still tied, choose the **first in DOM order**.
  5) Your final XPath for """"highlights"""" **must resolve to a single unique element**. If you cannot make it unique without using `contains()` or `starts-with()`, set """"highlights"""" to null.
- Do **not** return unions (no `|`) or multiple XPaths for highlights.

**Schedule Section**: Must contain actual itinerary items or schedule content, not just ""Schedule"" headers
- Look for: Day-by-day itineraries, timeline items, schedule entries, program details
- Validate: Contains actual schedule items, day descriptions, or itinerary content
- Avoid: Headers like ""Itinerary"", ""Schedule"" without actual schedule content
- If the exact @class you would use for the schedule container appears on multiple elements, you **must** disambiguate in this order, using only exact equality checks:
  1) Prefer a unique @id on the correct container if present (e.g., //*[@id=""""tour-schedule""""]).
  2) Anchor to an exact header text (e.g., """"Lịch trình"""", """"Chương trình"""", """"Itinerary"""") and select the **nearest following content container** that actually has content:
     - Example pattern (choose the appropriate level present in HTML):
       //*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5][normalize-space() = """"Lịch trình""""]/following-sibling::*[1][.//li or .//p]
  3) If no header anchor is available, combine exact class equality **plus a content constraint** to ensure you target the section with real schedule items:
     - e.g., //div[@class=""""EXACT-CLASS""""][.//li or .//*[@class=""""schedule-item""""] or .//p]
  4) If multiple candidates still remain, choose the element with the **largest count** of `.//li` or `.//*[@class=""""schedule-item""""]`; if still tied, choose the **first in DOM order**.
  5) Your final XPath for """"schedule"""" **must resolve to a single unique element**. If you cannot make it unique without using `contains()` or `starts-with()`, set """"schedule"""" to null.

IMPORTANT RULES:
**NULL IF NOT FOUND**: If a target section (images, single image, highlights, schedule, or popup trigger) does not exist in the HTML, or if a valid, unique XPath cannot be generated for it according to the rules below (e.g., no content, ambiguity that cannot be resolved), its corresponding value in the final JSON **must** be `null`.

**GLOBAL NON-DUPLICATE CLASS ENFORCEMENT (ALL SECTIONS)**:
- Use @class **only if** the element’s **exact full class string** appears **exactly once** in the entire document.
- The generator must **verify uniqueness** before using a class (internally check: count(//*[@class=""""THE-EXACT-CLASS""""]) = 1).
- If a class is duplicated (count > 1), **do not** use it — not even combined with other classes — and instead:
  1) Prefer a unique @id (//*[@id=""""...""""]).
  2) Then exact data-* (//*[@data-...=""""...""""]).
  3) Then exact aria-* with role (//*[@role=""""...""""][@aria-label=""""...""""]).
  4) Then **header-anchored** exact-text structure (e.g., //h2[normalize-space()=""""Điểm nhấn""""]/following-sibling::*[1][.//li or .//p]).
  5) Then other **non-class** exact-attribute combos (e.g., @data-*, @aria-*, @role) that yield a **single** node.
  6) Positional index [n] is allowed **only** as a last resort and **never** together with a duplicated class selector.
- If no unique selector can be formed without duplicated classes, set that field’s value to **null**.

- Use ACTUAL class names and structure from the provided HTML
- For regular content sections (highlights, schedule): Use exact class matching: //tagname[@class=""exact-full-classname""]
- For popup parent detection: Use exact class matching when possible, structural positioning as fallback
- Use alternative XPath strategies: ID matching, text matching, or structural positioning
- Only return XPaths for sections that actually exist in the HTML with CONTENT
- If a section header exists but no content is found, set its value to null
- **Never use contains() or starts-with()**; always use equality checks
- **DISAMBIGUATION WHEN CLASSES ARE DUPLICATED**:
  * If the same exact @class value appears on multiple elements such that it does not uniquely identify the target section, **switch to using a unique @id** on the correct container if available.
  * If a unique @id is not available, combine exact attributes to disambiguate (e.g., @data-*, @aria-*, @role) with equality checks.
  * If still ambiguous, anchor to an exact header text node (e.g., a preceding-sibling heading with exact text) and select its following content container using exact structure (no text contains()).
  * Positional indexing [n] is allowed only as a last resort when all above are unavailable.

**Selector Preference Order (highest to lowest):**
1) Unique @id equality (e.g., //*[@id = """"tour-highlights""""])
2) Exact data-* attribute equality (e.g., //*[@data-gallery = """"main""""])
3) Exact aria-* with role (e.g., //*[@role = """"region"""" and @aria-label = """"Gallery""""])
4) Exact class equality combined with exact child/descendant structure
5) Header-anchored exact-text structure (e.g., //h2[normalize-space()=""""Điểm nhấn""""]/following-sibling::*[1])
6) Positional index as last resort

XPath Strategies for Immediate Parent Detection:
1. Text-based approach: //span[text() = '+4']/parent::*
2. Structural approach: //*[span[text() = '+4']]
3. Class-based approach: //div[@class = 'exact-parent-class'][.//span[text() = '+4']]
4. Combined approach: //*[@class = 'gallery-item'][span[text() = '+4']]
5. Background-image thumbnails (exact match when present): //div[@class = 'thumb' and @style = 'background-image: url(EXACT_URL)']

POPUP IMMEDIATE PARENT EXAMPLES:
- Gallery item with overlay: <div class=""gallery-item""><img/><span>+4</span></div> → Target //div[@class = 'gallery-item']
- Clickable photo link: <a class=""photo-link""><div class=""thumb""></div><div>+3</div></a> → Target //a[@class = 'photo-link']
- Button trigger: <button class=""view-more""><span>+5 more</span></button> → Target //button[@class = 'view-more']

VALIDATION PROCESS:
- Before returning an XPath, verify it targets actual content containers in the provided HTML
- Use specific class names that appear exactly in the HTML for content sections
- Prefer exact matches that target content containers, not just headers
- Ensure the targeted element contains actual content (images, lists, schedule items)
- **FOR IMAGES: Prioritize immediate parent detection - if any ""+number"" indicators are found, return popupTriggerXpath instead of images**
- Never return both images and popupTriggerXpath at the same time

HTML Content:
{html}

Response format (return only JSON with actual working XPaths targeting CONTENT sections):
If immediate parent of +number element found:
{{
  ""images"": null,
  ""highlights"": ""/html//div[@id='exact-highlights-content-id-from-html']"",
  ""schedule"": ""/html//div[@id='exact-schedule-content-id-from-html']"",
  ""popupTriggerXpath"": ""/html//div[@id='exact-immediate-parent-id-from-html']"",
  ""image"" : null
}}

If no +number indicators found:
{{
  ""images"": ""/html//div[@id='exact-gallery-content-id-from-html']"",
  ""highlights"": ""/html//div[@id='exact-highlights-content-id-from-html']"",
  ""schedule"": ""/html//div[@id='exact-schedule-content-id-from-html']"",
  ""popupTriggerXpath"": null
  ""image"" : null
}}

If single primary image found:
{{
  ""images"": ""null"",
  ""highlights"": ""/html//div[@id='exact-highlights-content-id-from-html']"",
  ""schedule"": ""/html//div[@id='exact-schedule-content-id-from-html']"",
  ""popupTriggerXpath"": null
  ""image"": ""/html//img[@id='exact-single-image-id-from-html']""
}}";
            var response = await _geminiClient.GenerateContentAsync(fullPrompt);
            var result = response.Text;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (result.StartsWith("```"))
                {
                    var firstNewline = result.IndexOf('\n');
                    if (firstNewline > 0)
                    {
                        result = result.Substring(firstNewline + 1);
                    }

                    if (result.EndsWith("```"))
                    {
                        result = result.Substring(0, result.Length - 3);
                    }

                    result = result.Trim();
                }

                _logger.LogInformation("Tour program sections XPath result (no contains): {XPath}", result);
            }

            Dictionary<string, string> selectors;
            try
            {
                selectors = JsonConvert.DeserializeObject<Dictionary<string, string>>(result) ??
                            new Dictionary<string, string>();
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response as JSON: {Response}", result);
                selectors = new Dictionary<string, string>();
            }

            return RemoveNullValues(selectors);
        });
    }

    public async Task<string> GetParentSelectorAsync(string html)
    {
        return await RetryWithBackoffAsync<string>(async () =>
        {
            var truncatedHtml = html.Length > 110000 ? html.Substring(0, 110000) + "..." : html;

            var fullPrompt = $@"
You are a master-level DOM analysis specialist with expertise in identifying container hierarchies and parent-child relationships within complex HTML structures. Your mission is to locate the single, most appropriate parent container that serves as the primary wrapper for all tour item elements within the document.

**ADVANCED CONTAINER IDENTIFICATION METHODOLOGY:**

### **SEMANTIC CONTAINER ANALYSIS FRAMEWORK:**
1. **HIERARCHICAL STRUCTURE RECOGNITION**: Analyze DOM tree depth and nesting patterns to identify logical container boundaries
2. **CONTENT PATTERN VALIDATION**: Verify that identified containers actually contain multiple similar child elements representing individual tour items
3. **SEMANTIC MARKUP PRIORITIZATION**: Favor containers with meaningful semantic roles and accessible markup patterns
4. **LAYOUT CONTEXT UNDERSTANDING**: Consider responsive design patterns and grid-based layout structures
5. **DYNAMIC CONTENT AWARENESS**: Account for JavaScript-generated containers and progressive loading patterns

### **ENHANCED CONTAINER IDENTIFICATION STRATEGIES:**

#### **🎯 PRIMARY TARGETING CRITERIA:**

**Semantic Container Priority Hierarchy:**
1. **Semantic List Containers**: `<ul>`, `<ol>` elements containing multiple `<li>` items representing tours
2. **Section-Based Containers**: `<section>`, `<main>` elements that wrap comprehensive tour listing areas
3. **Grid/Flex Containers**: `<div>` elements with layout-specific classes (grid, flex, container) containing tour card patterns
4. **Content Wrappers**: Generic `<div>` elements that serve as immediate parents to multiple tour items

**Advanced Content Validation Requirements:**
- ✅ **MULTIPLE CHILDREN**: Must contain 2 or more direct child elements representing individual tour items
- ✅ **CONSISTENT STRUCTURE**: Child elements should have similar HTML structure and class patterns
- ✅ **CONTENT COMPLETENESS**: Children should contain comprehensive tour-related information (titles, prices, images, descriptions)
- ✅ **LOGICAL GROUPING**: Container should logically group related tour content without including peripheral navigation or UI elements
- ✅ **SEMANTIC APPROPRIATENESS**: Container should be semantically appropriate for its content type

#### **🔍 ADVANCED PATTERN RECOGNITION:**

**Class Name Semantic Analysis:**
- **List Indicators**: Classes containing 'list', 'grid', 'container', 'wrapper', 'items', 'cards', 'results'
- **Tour-Specific**: Classes with 'tour', 'product', 'item', 'content', 'result', 'listing', 'catalog'
- **Layout Patterns**: Classes indicating main content areas like 'main', 'primary', 'content-area', 'section'
- **Collection Indicators**: Classes suggesting content collections like 'collection', 'gallery', 'archive'

**Advanced Structural Validation:**
- **Direct Child Count**: Verify container has multiple direct children (minimum 2-3 tour items)
- **Child Consistency**: Ensure child elements have similar structure and class patterns
- **Content Density**: Prefer containers where children contain substantial tour-related content
- **Nesting Appropriateness**: Avoid over-nested or under-nested container selection

#### **📋 ENHANCED SELECTOR CONSTRUCTION RULES:**

**XPath Generation Strategy:**
1. **EXACT CLASS MATCHING**: Use precise class attribute matching without contains() functions for maximum specificity
2. **UNIQUE IDENTIFICATION**: Ensure generated XPath targets exactly one element in the document
3. **SEMANTIC PREFERENCE**: Prioritize semantically meaningful containers over generic wrappers
4. **PRODUCTION RELIABILITY**: Generate robust selectors that work across different page states and content variations
5. **ACCESSIBILITY COMPLIANCE**: Favor containers with proper ARIA attributes and semantic roles
6  **SINGLE ATTRIBUTE PRIORITY**: If a container has a unique @id attribute, use it: `//div[@id='unique-id']`
7. **UNIQUE CLASS PRIORITY**: If a container has a unique @class attribute, use it: `//div[@class='unique-class']`
8. **NO COMBINED CONDITIONS**: Never use combined conditions like `//div[@class='gridTour' and @id='mda-box-item-col-tour']`

**Enhanced Validation Hierarchy:**
1. **Unique ID Targeting**: `//*[@id='tour-list-container']` (highest priority if available)
2. **Semantic Elements**: `//section[@class='results']` or `//main[@id='content']`
3. **Exact Class Matching**: `//div[@class='find-tour-content__list--main']`
4. **Combined Attribute Matching**: `//div[@class='container'][@data-component='tour-list']`
5. **Role-Based Targeting**: `//*[@role='main']` or `//*[@id='main-content']`
6. **Unique Class Matching**: `//div[@class='tour-listing']` (use only if class is unique)

#### **⚡ COMPREHENSIVE QUALITY ASSURANCE CRITERIA:**

**Container Validation Checklist:**
- ✅ Contains multiple tour item children (minimum 2-3 items)
- ✅ Children have consistent structure and content patterns
- ✅ Container has unique identifying characteristics
- ✅ Not nested within other similar containers
- ✅ Positioned logically within page layout hierarchy
- ✅ Semantically appropriate for content type
- ✅ Accessible via screen readers and assistive technologies
- ✅ Works across different viewport sizes and device types

**Advanced Edge Case Handling:**
- **Nested Containers**: Select the most appropriate level that directly contains tour items without including irrelevant wrapper levels
- **Multiple Candidates**: Choose container with the most tour items, clearest semantic meaning, or best accessibility attributes
- **Dynamic Content**: Account for containers that may be populated via JavaScript or AJAX loading
- **Responsive Layouts**: Ensure selector works across different viewport sizes and responsive breakpoints
- **Progressive Enhancement**: Handle containers that may be enhanced with JavaScript functionality

#### **🎨 COMPREHENSIVE PRACTICAL EXAMPLES:**

**Advanced Container Patterns:**
```html
<!-- Semantic section-based layout -->
<section class=""tour-results"" role=""main"" aria-label=""Tour search results"">
  <article class=""tour-item"">...</article>
  <article class=""tour-item"">...</article>
</section>

<!-- Grid-based layout with semantic enhancement -->
<div class=""tour-grid-container"" role=""region"" aria-label=""Available tours"">
  <div class=""tour-card"" itemscope itemtype=""https://schema.org/TouristTrip"">...</div>
  <div class=""tour-card"" itemscope itemtype=""https://schema.org/TouristTrip"">...</div>
</div>

<!-- List-based layout with accessibility -->
<ul class=""tour-listing"" role=""list"" aria-describedby=""tour-count"">
  <li class=""tour-item"" role=""listitem"">...</li>
  <li class=""tour-item"" role=""listitem"">...</li>
</ul>

<!--  -->
<div class=""tourListContainerHeader"">
    <div class=""tourListContent "">
        <div class=""tourList"">
            <div class=""tourItem "">...</div>
            <div class=""tourItem "">...</div>
            <div class=""tourItem "">...</div>
        </div>
        <div class=""text-center"">
            <nav>...</nav>
        </div>
    </div>
</div>

<!-- Main content wrapper -->
<main class=""content-area__tour-results"" id=""main-content"">
  <div class=""tour-result-card"">...</div>
  <div class=""tour-result-card"">...</div>
</main>

**Expected XPath Outputs (Prioritized):**
- `//section[@class='tour-results']` (semantic with role)
- `//div[@class='tour-grid-container']` (semantic container)
- `//ul[@class='tour-listing']` (semantic list)
- `//main[@class='content-area__tour-results']` (main content area)
- `//div[@class='tourList']` (semantic list )

#### **🚨 CRITICAL REQUIREMENTS & QUALITY STANDARDS:**

**Response Format Requirements:**
- Return **ONLY** the XPath expression as a plain string
- **NO** explanations, comments, or additional text
- **NO** markdown formatting or code blocks
- **EXACT** class matching without contains() functions
- **ABSOLUTE** XPath starting with `//` 
- **PRODUCTION-READY** selector that works reliably

**Advanced Validation Standards:**
- XPath must resolve to exactly **ONE unique element** in the document
- Selected element must be the **direct parent** of tour items, not a grandparent or ancestor
- Container must contain **multiple similar child elements** representing tour content
- Generated selector must be **robust** against minor HTML structure changes
- Selector must work across **different device types** and **viewport sizes**
- Should leverage **semantic HTML** and **accessibility attributes** when available

**Performance & Reliability Considerations:**
- Avoid overly complex XPath expressions that could impact evaluation performance
- Prefer direct, efficient selectors over deeply nested traversals
- Ensure compatibility with modern web crawling and automation tools
- Generate selectors that work consistently across different browser engines

HTML Content for Analysis:
{html}

**Expected Response:** Return only the XPath expression (example: `//div[@class='tour-list-container']`)";

            var response = await _geminiClient.GenerateContentAsync(fullPrompt);
            var result = response.Text;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (result.StartsWith("```"))
                {
                    var firstNewline = result.IndexOf('\n');
                    if (firstNewline > 0)
                    {
                        result = result.Substring(firstNewline + 1);
                    }

                    if (result.EndsWith("```"))
                    {
                        result = result.Substring(0, result.Length - 3);
                    }

                    result = result.Trim();
                }

                _logger.LogInformation("Cleaned XPath result: {XPath}", result);
            }

            return result;
        });
    }

    public async Task<Dictionary<string, string>> GetXpathBasicSummaryAndParent(string html)
    {
        return await RetryWithBackoffAsync<Dictionary<string, string>>(async () =>
        {
            var prompt = $@"
You are an expert XPath generator. Return only valid JSON without explanations or markdown formatting.

You are an expert in HTML structure and XPath generation. Analyze the provided HTML content carefully and extract XPath selectors based on the ACTUAL class names and structure present in the HTML.

XPATH GENERATION STRATEGY:
**PRIMARY REQUIREMENT**: Generate XPaths for child elements starting from the parent element
- **STEP 1**: Identify the parent container that wraps all tour detail fields
- **STEP 2**: For each field, create XPaths that are RELATIVE to this parent element
- **STEP 3**: Use descendant selectors (.//tagname) to find elements within the parent context
- **STEP 4**: Ensure all field XPaths work when applied to individual parent containers

PARENT-RELATIVE XPATH RULES:
- Parent XPath should be absolute: //div[@class='card-filter-desktop']
- All child field XPaths should be relative to parent: .//div[@class='child-element']
- Child XPaths should work when applied to the parent element context
- Use descendant axis (.//tagname) for child element selection
- Avoid absolute paths (//) for child elements - use relative paths instead

CRITICAL VALIDATION INSTRUCTIONS FOR ALL FIELDS:
1. **STRUCTURAL ACCURACY**: Every XPath MUST be constructed using the exact class names, IDs, attributes, and element tags found in the provided HTML. All selectors must correspond directly to the document's structure. Do not invent, guess, or generalize selectors that are not explicitly present.
2. ALWAYS validate that your XPath targets elements containing ACTUAL VALUES, not just labels.
3. If you find a label element (like """"Mã tour:"""", """"Thời gian:"""", """"Khởi hành:"""", """"Giá từ:""""), you must follow it to find the adjacent, child, or sibling element that contains the actual value.
4. Elements must contain meaningful data content, not just descriptive labels.
5. Look for patterns in the actual content to identify value-containing elements.

SPECIFIC FIELD VALIDATION RULES:

**Parent**: Must be the container element that wraps all tour information, not just a label container
- Look for: div elements that contain multiple tour info sections as children
- Validate: Contains child elements with tour data, not just labels
- FORMAT: Absolute XPath (//div[@class='parent-class'])

**Tour Code**: Must contain actual alphanumeric codes, not labels like ""Mã tour:"" or ""Tour code:""
- Look for: Elements with patterns like ""VN001"", ""TOUR123"", etc. within the parent
- Validate: Contains alphanumeric codes, not just labels
- FORMAT: Relative XPath (.//tagname[@class='code-class'])

**Itinerary** : Must contain actual itinerary details, not just labels like ""Hành trình:"" or ""Itinerary:""
- Look for: Elements that contain detailed itinerary information, such as day-by-day descriptions, activities, or schedules
- Validate: Contains structured itinerary content, not just labels
- Examples: ""Seoul - Nami - Lotte World - Busan - Gyeongju""
- FORMAT: Relative XPath (.//tagname[@class='itinerary-class'])

**Departure Location**: Must contain actual location names, not labels like ""Khởi hành:"" or ""Departure:""
- Look for: City/location names like ""Hà Nội"", ""TP.HCM"", ""Hanoi"", etc. within the parent
- Validate: Contains location names, not just labels
- FORMAT: Relative XPath (.//tagname[@class='departure-class'])

**Duration**: Must contain actual time periods, not labels like ""Thời gian:"" or ""Duration:""
- Look for: Patterns like ""3 ngày 2 đêm"", ""5 days 4 nights"", ""2D1N"", etc. within the parent
- Validate: Contains numbers with time units, not just labels
- FORMAT: Relative XPath (.//tagname[@class='duration-class'])

**Transportation**: Must contain actual transport types, not labels like ""Phương tiện:"" or ""Transport:""
- Look for: Transport modes like ""Máy bay"", ""Xe khách"", ""Flight"", ""Bus"", aircraft brand names (e.g., ""Vietnam Airlines"", ""Bamboo Airways"", ""VietJet Air""), etc. within the parent
- Validate: Contains transport method names, not just labels
- FORMAT: Relative XPath (.//tagname[@class='transport-class'])

**Departure Dates**: Must contain actual dates/date ranges, not labels like ""Ngày khởi hành:"" or ""Dates:""
- Look for: Date formats, calendars, date selectors with actual dates within the parent
- Validate: Contains date information, not just labels
- FORMAT: Relative XPath (.//tagname[@class='dates-class'])

**Departure Time**: Must contain actual time values, not labels like ""Giờ khởi hành:"" or ""Time:""
- Look for: Time formats like ""06:00"", ""8:30 AM"", etc. within the parent
- Validate: Contains time values, not just labels
- FORMAT: Relative XPath (.//tagname[@class='time-class'])

**Price**: Must contain actual numeric price values with currency, not labels like ""Giá từ:"" or ""Price:""
- Look for: Numbers with currency symbols/words (VND, $, đ, USD, etc.) within the parent
- Validate: Contains numeric values with currency information
- Examples: ""1,500,000 VND"", ""$299"", ""2.500.000đ""
- FORMAT: Relative XPath (.//tagname[@class='price-class'])

**Price Old**: Must be an ACTUAL old/original price with currency.
Return null if:
- No digits present, OR
- No currency marker present, OR
- Text is label-only (e.g., """"Giá từ"""", """"Giá gốc"""", """"Giá cũ"""", """"Price"""", """"From""""), OR
- The element is empty or whitespace.
Look for typical old-price styling: classes like 'old', 'strike', 'line-through', 'price-old', 'original-price', or <del>, <s>.
XPath must enforce:
  (A) at least one digit: translate(normalize-space(.), '0123456789', '') != normalize-space(.)
  (B) a currency marker: contains(., 'đ') or contains(., 'VND') or contains(., '$') or contains(., 'USD')
  (C) not a label-only phrase: not(contains(., 'Giá từ')) and not(contains(., 'Giá gốc')) and not(contains(., 'Giá cũ')) and not(contains(., 'Price')) and not(contains(., 'From'))
- FORMAT (example, adjust tag/class names to the page):
  .//*[self::del or self::s or contains(@class,'old') or contains(@class,'strike') or contains(@class,'line-through') or contains(@class,'price-old') or contains(@class,'original-price')]
    [normalize-space(.) != '']
    [translate(normalize-space(.), '0123456789', '') != normalize-space(.)]
    [contains(., 'đ') or contains(., 'VND') or contains(., '$') or contains(., 'USD')]
    [not(contains(., 'Giá từ')) and not(contains(., 'Giá gốc')) and not(contains(., 'Giá cũ')) and not(contains(., 'Price')) and not(contains(., 'From'))]
IMPORTANT RULES:
**GLOBAL NON-DUPLICATE CLASS ENFORCEMENT (ALL SECTIONS)**:
- Use @class **only if** the element’s **exact full class string** appears **exactly once** in the entire document.
- The generator must **verify uniqueness** before using a class (internally check: count(//*[@class=""""THE-EXACT-CLASS""""]) = 1).
- If a class is duplicated (count > 1), **do not** use it — not even combined with other classes — and instead:
  1) Prefer a unique @id (//*[@id=""""...""""]).
  2) Then exact data-* (//*[@data-...=""""...""""]).
  3) Then exact aria-* with role (//*[@role=""""...""""][@aria-label=""""...""""]).
  4) Then **header-anchored** exact-text structure (e.g., //h2[normalize-space()=""""Điểm nhấn""""]/following-sibling::*[1][.//li or .//p]).
  5) Then other **non-class** exact-attribute combos (e.g., @data-*, @aria-*, @role) that yield a **single** node.
  6) Positional index [n] is allowed **only** as a last resort and **never** together with a duplicated class selector.
- If no unique selector can be formed without duplicated classes, set that field’s value to **null**.

- Use ACTUAL class names and structure from the provided HTML
- For regular content sections (highlights, schedule): Use exact class matching: //tagname[@class=""exact-full-classname""]
- For popup parent detection: Use exact class matching when possible, structural positioning as fallback
- Use alternative XPath strategies: ID matching, text matching, or structural positioning
- Only return XPaths for sections that actually exist in the HTML with CONTENT
- If a section header exists but no content is found, set its value to null
- **Never use contains() or starts-with()**; always use equality checks
- **DISAMBIGUATION WHEN CLASSES ARE DUPLICATED**:
  * If the same exact @class value appears on multiple elements such that it does not uniquely identify the target section, **switch to using a unique @id** on the correct container if available.
  * If a unique @id is not available, combine exact attributes to disambiguate (e.g., @data-*, @aria-*, @role) with equality checks.
  * If still ambiguous, anchor to an exact header text node (e.g., a preceding-sibling heading with exact text) and select its following content container using exact structure (no text contains()).
  * Positional indexing [n] is allowed only as a last resort when all above are unavailable.

GENERAL VALIDATION PROCESS:
1. First, identify the parent container that wraps all tour fields (use absolute XPath)
2. Then, for each child field, create relative XPaths that work within the parent context
3. Use descendant axis (.//tagname) to search within the parent element
4. Target value elements, not label elements
5. Verify each child XPath works when applied to the parent element context

XPATH STRATEGIES FOR CHILD ELEMENTS:
- Parent context: //div[@class='parent-class'] (absolute)
- Child elements: .//tagname[@class='child-class'] (relative to parent)
- Attribute extraction: .//tagname[@class='child-class']/@attribute
- Text content: .//tagname[@class='child-class']/text()
- Multiple elements: .//tagname[@class='child-class'] (returns all matches within parent)

PARENT-CHILD RELATIONSHIP EXAMPLES:
- Parent: //div[@class='card-filter-desktop']
- Tour Code child: .//div[contains(@class,'info-tour-tourCode')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Departure Location child: .//div[contains(@class,'info-tour-departure')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Duration child: .//div[contains(@class,'info-tour-dayStayText--time')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Price child: .//div[@class='card-filter-desktop__content--price-newPrice']/p
- Link child: .//div[@class='card-filter-desktop__content--price-btn']/a/@href

IMPORTANT:
- Use ONLY the class names, IDs, and HTML structure that actually exist in the provided HTML
- Do NOT use generic class names like 'tour-summary', 'info-field', 'value' etc.
- Look for patterns in the actual HTML structure to identify tour information sections
- Generate XPath expressions that target the real HTML elements containing tour data
- Parent XPath should be absolute (//tagname), all child XPaths should be relative (.//tagname)

Your task:
1. Find the XPath of the parent element that contains the basic summary of the tour (use absolute XPath)
2. For each info field, generate RELATIVE XPath expressions that work within the parent context

Look for elements that contain text patterns like:
- Tour codes (usually alphanumeric codes)
- Duration (days/nights format)
- Departure Location (city names)
- Transportation types
- Dates and times
- Price information

Instructions:
- Examine the actual class names and HTML structure in the provided content
- Return a JSON object with keys: 'parent', 'tourCode', 'departure', 'duration', 'transportation', 'dates', 'departureTime', 'price', 'detailsLink', 'priceOld'
- If you cannot find a specific field in the HTML, set its value to null
- Only return the JSON object, no explanations

HTML Content:
{html}

Response format example (Parent is absolute, all child elements are relative to parent):
{{
  ""parent"": ""//div[@class='card-filter-desktop']"",
  ""tourCode"": "".//div[contains(@class,'info-tour-tourCode')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""itinerary"" : "".//div[contains(@class,'info-tour-itinerary')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""departureLocation"": "".//div[contains(@class,'info-tour-departure')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""duration"": "".//div[contains(@class,'info-tour-dayStayText--time')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""transportation"": "".//div[contains(@class,'info-tour-dayStayText') and not(contains(@class,'info-tour-dayStayText--time'))]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""departureDates"": "".//div[contains(@class,'info-tour-calendar')]//div[@class='list-item']/a"",
  ""departureTime"": null,
  ""price"": "".//div[@class='card-filter-desktop__content--price-newPrice']/p"",
  ""priceOld"": "".//div[contains(@class,'content--price-oldPrice') or contains(@class,'price-old') or contains(@class,'original-price')]//*[last()][contains(text(),'VND') or contains(text(),'$') or contains(text(),'đ')]""
}}";
            var response = await _geminiClient.GenerateContentAsync(prompt);
            var result = response.Text;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (result.StartsWith("```"))
                {
                    var firstNewline = result.IndexOf('\n');
                    if (firstNewline > 0)
                    {
                        result = result.Substring(firstNewline + 1);
                    }

                    if (result.EndsWith("```"))
                    {
                        result = result.Substring(0, result.Length - 3);
                    }

                    result = result.Trim();
                }

                _logger.LogInformation("Basic summary selectors JSON: {Selectors}", result);
            }

            Dictionary<string, string> selectors;
            try
            {
                selectors = JsonConvert.DeserializeObject<Dictionary<string, string>>(result) ??
                            new Dictionary<string, string>();
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response as JSON: {Response}", result);
                selectors = new Dictionary<string, string>();
            }

            return RemoveNullValues(selectors);
        });
    }

    public async Task<Dictionary<string, string>> GetXpathOfHtmlByRequest(string html, string request)
    {
        return null;
    }


    public async Task<Dictionary<string, string>> GetTourDetailSelectorsAsync(string html)
    {
        return await RetryWithBackoffAsync<Dictionary<string, string>>(async () =>
        {
            var prompt = $@"
You are an expert XPath generator. Return only valid JSON without explanations or markdown formatting.

You are an expert XPath generator. Analyze the provided HTML and extract the XPath for each of the following tour details:
- Parent
- Title
- TourCode
- DepartureLocation
- Duration
- Transportation
- DepartureDates
- DepartureTime
- Price
- DetailsLink
- PriceOld
- Thumbnail
- Itinerary 

CRITICAL VALIDATION INSTRUCTIONS FOR ALL FIELDS:
1. **STRUCTURAL ACCURACY**: Every XPath MUST be constructed using the exact class names, IDs, attributes, and element tags found in the provided HTML. All selectors must correspond directly to the document's structure. Do not invent, guess, or generalize selectors that are not explicitly present.
2. ALWAYS validate that your XPath targets elements containing ACTUAL VALUES, not just labels.
3. If you find a label element (like """"Mã tour:"""", """"Thời gian:"""", """"Khởi hành:"""", """"Giá từ:""""), you must follow it to find the adjacent, child, or sibling element that contains the actual value.
4. Elements must contain meaningful data content, not just descriptive labels.
5. Look for patterns in the actual content to identify value-containing elements.

XPATH GENERATION STRATEGY:
**PRIMARY REQUIREMENT**: Generate XPaths for child elements starting from the parent element
- **STEP 1**: Identify the parent container that wraps all tour detail fields
- **STEP 2**: For each field, create XPaths that are RELATIVE to this parent element
- **STEP 3**: Use descendant selectors (.//tagname) to find elements within the parent context
- **STEP 4**: Ensure all field XPaths work when applied to individual parent containers

PARENT-RELATIVE XPATH RULES:
- Parent XPath should be absolute: //div[@class='card-filter-desktop']
- All child field XPaths should be relative to parent: .//div[@class='child-element']
- Child XPaths should work when applied to the parent element context
- Use descendant axis (.//tagname) for child element selection
- Avoid absolute paths (//) for child elements - use relative paths instead

SPECIFIC FIELD VALIDATION RULES:

**Parent**: Must be the single container that wraps all tour detail fields
- Look for: div elements that contain multiple tour detail sections as direct or indirect children
- Validate: Contains child elements for title, tour code, departure, duration, price, etc.
- Usually has classes related to 'card', 'item', 'tour', 'content', 'wrapper'
- FORMAT: Absolute XPath (//div[@class='parent-class'])

**Title**: Must contain actual tour title text, not labels like ""Tour title:"" or ""Tên tour:""
- Look for: h1, h2, h3 elements with tour names within the parent
- Validate: Contains descriptive tour text, not just labels
- FORMAT: Relative XPath (.//tagname[@class='title-class'])

**Tour Code**: Must contain actual alphanumeric codes, not labels like ""Mã tour:"" or ""Tour code:""
- Look for: Elements with patterns like ""VN001"", ""TOUR123"", etc. within the parent
- Validate: Contains alphanumeric codes, not just labels
- FORMAT: Relative XPath (.//tagname[@class='code-class'])

**Departure Location**: Must contain actual location names, not labels like ""Khởi hành:"" or ""Departure:""
- Look for: City/location names like ""Hà Nội"", ""TP.HCM"", ""Hanoi"", etc. within the parent
- Validate: Contains location names, not just labels
- FORMAT: Relative XPath (.//tagname[@class='departure-class'])

**Duration**: Must contain actual time periods, not labels like ""Thời gian:"" or ""Duration:""
- Look for: Patterns like ""3 ngày 2 đêm"", ""5 days 4 nights"", ""2D1N"", etc. within the parent
- Validate: Contains numbers with time units, not just labels
- FORMAT: Relative XPath (.//tagname[@class='duration-class'])

**Transportation**: Must contain actual transport types, not labels like ""Phương tiện:"" or ""Transport:""
- Look for: Transport modes like ""Máy bay"", ""Xe khách"", ""Flight"", ""Bus"", aircraft brand names (e.g., ""Vietnam Airlines"", ""Bamboo Airways"", ""VietJet Air""), etc. within the parent
- Validate: Contains transport method names, not just labels
- FORMAT: Relative XPath (.//tagname[@class='transport-class'])

**Departure Dates**: Must contain actual dates/date ranges, not labels like ""Ngày khởi hành:"" or ""Dates:""
- Look for: Date formats, calendars, date selectors with actual dates within the parent 
- Validate: Contains date information, not just labels
- FORMAT: Relative XPath (.//tagname[@class='dates-class'])

**Departure Time**: Must contain actual time values, not labels like ""Giờ khởi hành:"" or ""Time:""
- Look for: Time formats like ""06:00"", ""8:30 AM"", etc. within the parent
- Validate: Contains time values, not just labels
- FORMAT: Relative XPath (.//tagname[@class='time-class'])

**Price**: Must contain actual numeric price values with currency, not labels like ""Giá từ:"" or ""Price:""
- Look for: Numbers with currency symbols/words (VND, $, đ, USD, etc.) within the parent
- Validate: Contains numeric values with currency information
- Examples: ""1,500,000 VND"", ""$299"", ""2.500.000đ""
- FORMAT: Relative XPath (.//tagname[@class='price-class'])

**Itinerary** : Must contain actual itinerary details, not just labels like ""Hành trình:"" or ""Itinerary:""
- Look for: Elements that contain detailed itinerary information, such as day-by-day descriptions, activities, or schedules
- Validate: Contains structured itinerary content, not just labels
- Examples: ""Seoul - Nami - Lotte World - Busan - Gyeongju""
- FORMAT: Relative XPath (.//tagname[@class='itinerary-class'])

**Details Link**: Must contain actual URL/href attributes, not just link labels
- **FLEXIBLE STRATEGY**: Use a multi-priority approach to find the most relevant details link:
  - **PRIORITY 1**: Use the link inside the title element if available and valid (highest priority)
  - **PRIORITY 2**: Look for dedicated details/view buttons with non-empty href attributes (.//a[contains(@class,'detail') or contains(@class,'view') or contains(@class,'more')]/@href)
  - **PRIORITY 3**: As fallback, find any anchor tag with non-empty href within the parent that appears to be a navigation link
- **VALIDATION REQUIREMENTS**:
  - The href attribute must contain an actual URL or path (not empty, not ""#"", not ""javascript:void(0)"")
  - Prioritize links that seem to lead to detail pages (containing ""detail"", ""view"", ""more"", or similar indicators)
  - Avoid links that are clearly for other purposes (social media, external sites, etc.)
- **XPATH STRATEGY**: Use XPath union (|) to check multiple possible locations:
  - Example: (.//h2/a/@href | .//a[contains(@class,'btn-detail')]/@href | .//div[@class='title-wrapper']/a/@href)[normalize-space(.) != '' and . != '#'][1]
- FORMAT: Relative XPath with fallback options (.//multiple-selector-options/@href)

**Price Old**: Must be an ACTUAL old/original price with currency.
Return null if:
- No digits present, OR
- No currency marker present, OR
- Text is label-only (e.g., """"Giá từ"""", """"Giá gốc"""", """"Giá cũ"""", """"Price"""", """"From""""), OR
- The element is empty or whitespace.
Look for typical old-price styling: classes like 'old', 'strike', 'line-through', 'price-old', 'original-price', or <del>, <s>.
XPath must enforce:
  (A) at least one digit: translate(normalize-space(.), '0123456789', '') != normalize-space(.)
  (B) a currency marker: contains(., 'đ') or contains(., 'VND') or contains(., '$') or contains(., 'USD')
  (C) not a label-only phrase: not(contains(., 'Giá từ')) and not(contains(., 'Giá gốc')) and not(contains(., 'Giá cũ')) and not(contains(., 'Price')) and not(contains(., 'From'))
- FORMAT (example, adjust tag/class names to the page):
  .//*[self::del or self::s or contains(@class,'old') or contains(@class,'strike') or contains(@class,'line-through') or contains(@class,'price-old') or contains(@class,'original-price')]
    [normalize-space(.) != '']
    [translate(normalize-space(.), '0123456789', '') != normalize-space(.)]
    [contains(., 'đ') or contains(., 'VND') or contains(., '$') or contains(., 'USD')]
    [not(contains(., 'Giá từ')) and not(contains(., 'Giá gốc')) and not(contains(., 'Giá cũ')) and not(contains(., 'Price')) and not(contains(., 'From'))]

**Thumbnail**: Must contain actual image src URLs, not just image containers or labels
- Look for: img elements with src attributes containing actual image URLs within the parent
- Validate: Contains actual image source URLs (jpg, png, webp, etc.)
- Target: The main tour image, hero image, or primary thumbnail
- Priority: Look for main tour images, hero images, or featured images first
- Examples: src=""https://example.com/tour-image.jpg"", src=""/images/tour-thumb.webp""
- FORMAT: Relative XPath using a union over attributes and selecting the first non-empty result:
(.//img[@class='image-class']/@data-src
| .//img[@class='image-class']/@data-original
| .//img[@class='image-class']/@data-lazy
| .//img[@class='image-class']/@data-ll-src
| .//img[@class='image-class']/@data-srcset
| .//img[@class='image-class']/@srcset
| .//img[@class='image-class']/@src)[normalize-space(.) != ''][1]

IMPORTANT RULES:
**GLOBAL NON-DUPLICATE CLASS ENFORCEMENT (ALL SECTIONS)**:
- Use @class **only if** the element’s **exact full class string** appears **exactly once** in the entire document.
- The generator must **verify uniqueness** before using a class (internally check: count(//*[@class=""""THE-EXACT-CLASS""""]) = 1).
- If a class is duplicated (count > 1), **do not** use it — not even combined with other classes — and instead:
  1) Prefer a unique @id (//*[@id=""""...""""]).
  2) Then exact data-* (//*[@data-...=""""...""""]).
  3) Then exact aria-* with role (//*[@role=""""...""""][@aria-label=""""...""""]).
  4) Then **header-anchored** exact-text structure (e.g., //h2[normalize-space()=""""Điểm nhấn""""]/following-sibling::*[1][.//li or .//p]).
  5) Then other **non-class** exact-attribute combos (e.g., @data-*, @aria-*, @role) that yield a **single** node.
  6) Positional index [n] is allowed **only** as a last resort and **never** together with a duplicated class selector.
- If no unique selector can be formed without duplicated classes, set that field’s value to **null**.

- Use ACTUAL class names and structure from the provided HTML
- For regular content sections (highlights, schedule): Use exact class matching: //tagname[@class=""exact-full-classname""]
- For popup parent detection: Use exact class matching when possible, structural positioning as fallback
- Use alternative XPath strategies: ID matching, text matching, or structural positioning
- Only return XPaths for sections that actually exist in the HTML with CONTENT
- If a section header exists but no content is found, set its value to null
- **Never use contains() or starts-with()**; always use equality checks
- **DISAMBIGUATION WHEN CLASSES ARE DUPLICATED**:
  * If the same exact @class value appears on multiple elements such that it does not uniquely identify the target section, **switch to using a unique @id** on the correct container if available.
  * If a unique @id is not available, combine exact attributes to disambiguate (e.g., @data-*, @aria-*, @role) with equality checks.
  * If still ambiguous, anchor to an exact header text node (e.g., a preceding-sibling heading with exact text) and select its following content container using exact structure (no text contains()).
  * Positional indexing [n] is allowed only as a last resort when all above are unavailable.

GENERAL VALIDATION PROCESS:
1. First, identify the parent container that wraps all tour fields (use absolute XPath)
2. Then, for each child field, create relative XPaths that work within the parent context
3. Use descendant axis (.//tagname) to search within the parent element
4. Target value elements, not label elements
5. Verify each child XPath works when applied to the parent element context

XPATH STRATEGIES FOR CHILD ELEMENTS:
- Parent context: //div[@class='parent-class'] (absolute)
- Child elements: .//tagname[@class='child-class'] (relative to parent)
- Attribute extraction: .//tagname[@class='child-class']/@attribute
- Text content: .//tagname[@class='child-class']/text()
- Multiple elements: .//tagname[@class='child-class'] (returns all matches within parent)

PARENT-CHILD RELATIONSHIP EXAMPLES:
- Parent: //div[@class='card-filter-desktop']
- Tour Code child: .//div[contains(@class,'info-tour-tourCode')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Departure Location child: .//div[contains(@class,'info-tour-departure')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Duration child: .//div[contains(@class,'info-tour-dayStayText--time')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']
- Price child: .//div[@class='card-filter-desktop__content--price-newPrice']/p
- Link child: .//div[@class='card-filter-desktop__content--price-btn']/a/@href

IMPORTANT:
- Use ONLY the class names, IDs, and HTML structure that actually exist in the provided HTML
- Do NOT use generic class names like 'tour-summary', 'info-field', 'value' etc.
- Look for patterns in the actual HTML structure to identify tour information sections
- Generate XPath expressions that target the real HTML elements containing tour data
- Parent XPath should be absolute (//tagname), all child XPaths should be relative (.//tagname)

Your task:
1. Find the XPath of the parent element that contains the basic summary of the tour (use absolute XPath)
2. For each info field, generate RELATIVE XPath expressions that work within the parent context

Look for elements that contain text patterns like:
- Tour codes (usually alphanumeric codes)
- Duration (days/nights format)
- Departure Location (city names)
- Transportation types
- Dates and times
- Price information

Instructions:
- Examine the actual class names and HTML structure in the provided content
- Return a JSON object with keys: 'parent', 'tourCode', 'departure', 'duration', 'transportation', 'dates', 'departureTime', 'price', 'detailsLink', 'priceOld'
- If you cannot find a specific field in the HTML, set its value to null
- Only return the JSON object, no explanations

HTML Content:
{html}

Response format example (Parent is absolute, all child elements are relative to parent):
{{
  ""parent"": ""//div[@class='card-filter-desktop']"",
  ""tourCode"": "".//div[contains(@class,'info-tour-tourCode')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""thumbnail"": "".//div[contains(@class,'image-class']//img/@data-src | .//div[contains(@class,'image-class']//img/@data-original | .//div[contains(@class,'image-class']//img/@data-lazy | .//div[contains(@class,'image-class']//img/@data-ll-src | .//div[contains(@class,'image-class']//img/@data-srcset | .//div[contains(@class,'image-class']//img/@srcset | .//div[contains(@class,'image-class']//img/@src)[normalize-space(.) != ''][1]""
  ""title"": "".//h1[@class='card-filter-desktop__content--title']"",
  ""itinerary"" : "".//div[contains(@class,'info-tour-itinerary')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""departureLocation"": "".//div[contains(@class,'info-tour-departure')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""duration"": "".//div[contains(@class,'info-tour-dayStayText--time')]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""transportation"": "".//div[contains(@class,'info-tour-dayStayText') and not(contains(@class,'info-tour-dayStayText--time'))]//p[@class='card-filter-desktop__content--info-tour--item-wrapper-content']"",
  ""departureDates"": "".//div[contains(@class,'info-tour-calendar')]//div[@class='list-item']/a"",
  ""departureTime"": null,
  ""price"": "".//div[@class='card-filter-desktop__content--price-newPrice']/p"",
  ""priceOld"": "".//div[contains(@class,'content--price-oldPrice') or contains(@class,'price-old') or contains(@class,'original-price')]//*[last()][contains(text(),'VND') or contains(text(),'$') or contains(text(),'đ')]""
}}";
            var response = await _geminiClient.GenerateContentAsync(prompt);
            var result = response.Text;

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.Trim();
                if (result.StartsWith("```"))
                {
                    var firstNewline = result.IndexOf('\n');
                    if (firstNewline > 0)
                    {
                        result = result.Substring(firstNewline + 1);
                    }

                    if (result.EndsWith("```"))
                    {
                        result = result.Substring(0, result.Length - 3);
                    }

                    result = result.Trim();
                }

                _logger.LogInformation("Tour detail selectors JSON: {Selectors}", result);
            }

            Dictionary<string, string> selectors;
            try
            {
                selectors = JsonConvert.DeserializeObject<Dictionary<string, string>>(result) ??
                            new Dictionary<string, string>();
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response as JSON: {Response}", result);
                selectors = new Dictionary<string, string>();
            }

            return RemoveNullValues(selectors);
        });
    }

    private async Task<T> RetryWithBackoffAsync<T>(Func<Task<T>> operation, int maxRetries = 5)
    {
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsRetryableException(ex))
            {
                _logger.LogWarning(
                    "Gemini API unavailable or rate limited (attempt {Attempt}), retrying in {Delay}ms",
                    attempt + 1, delay.TotalMilliseconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini API call failed (attempt {Attempt}), retrying in {Delay}ms",
                    attempt + 1, delay.TotalMilliseconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }

        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed after retries.");
            return default!;
        }
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex.Message.Contains("429") ||
               ex.Message.Contains("503") ||
               ex.Message.Contains("RATE_LIMIT_EXCEEDED") ||
               ex.Message.Contains("UNAVAILABLE");
    }

    private static Dictionary<string, string> RemoveNullValues(Dictionary<string, string> dictionary)
    {
        return dictionary.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "null")
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}