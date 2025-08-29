# CrawlProject

## Overview

CrawlProject is an ASP.NET Core web application designed for web crawling, data extraction, and exporting results to
Excel. It integrates AI-powered content analysis and supports custom data selection via RESTful API endpoints.

## Features

- Web crawling and data extraction
- Custom data selection
- Export crawled data to Excel (.xlsx)
- AI-powered content analysis (Gemini integration)
- Robust error handling

## Prerequisites

- .NET 9.0 SDK
- Chrome browser
- MongoDB (if using database features) *incomplete
- Gemini API key (required for AI-powered features)

## Installation

1. Clone the repository:
   ```sh
   git clone <your-repo-url>
   ```
2. Restore NuGet packages:
   ```sh
   dotnet restore
   ```
3. Build the project:
   ```sh
   dotnet build
   ```
4. Update configuration files (`appsettings.json`, `appsettings.Development.json`) as needed.
    - Add your Gemini API key to `appsettings.json`:

 ```json
 {
  "Gemini": {
    "ApiKey": "<your-gemini-api-key>"
  }
}
 ```

## Usage

Run the application:

```sh
dotnet run
```

The API will be available at `https://localhost:5115`.

## API Endpoints

- `POST /CrawlData` - Crawl data and export to Excel
- `POST /GetSelects` - Get select options for crawling
- `POST /CustomSelect` - Custom data selection
- Additional endpoints may be available; see controller files for details.

## Project Structure

- `Controllers/` - API controllers
- `Services/` - Business logic and integrations
- `Dto/` - Data transfer objects
- `Interfaces/` - Service interfaces
- `Models/` - Data models
- `Utils/` - Utility classes
- `Config/` - Configuration classes

## Technologies Used

- ASP.NET Core
- EPPlus (Excel export)
- AngleSharp, HtmlAgilityPack (HTML parsing)
- GenerativeAI (Gemini integration)
- MongoDB
- Selenium.WebDriver, Selenium.WebDriver.ChromeDriver (Chrome automation)


