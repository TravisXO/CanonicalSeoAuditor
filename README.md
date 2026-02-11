**Canonical SEO Auditor**

A powerful, full-stack SEO analysis tool designed to crawl, render, and grade websites against modern search engine standards.

Built with .NET 8 for the robust analysis engine and Node.js/Puppeteer for headless browser rendering, ensuring even JavaScript-heavy Single Page Applications (SPAs) are audited accurately.

**🚀 Features**

**The auditor performs a 12-Point Comprehensive Check covering over 50 specific metrics:**

- **Metadata Analysis:** Title tags, descriptions, viewports, and charsets.

- **Content Structure:** Word counts, H1-H6 hierarchy, text-to-HTML ratios.

- **Technical SEO:** Canonical tags, Hreflang attributes, Favicons, Preconnects.

- **Accessibility:** ARIA landmarks, skip links, tab index issues.

- **Performance:** DOM complexity, deprecated HTML tags, inline styles.

- **Images:** Alt text, aspect ratios (CLS), modern formats (WebP/AVIF), lazy loading.

- **Social Graph:** Open Graph (Facebook/LinkedIn) and Twitter Card validation.

- **Schema Markup:** JSON-LD detection for Articles, Products, Breadcrumbs, etc.

- **Content Quality:** Flesch Reading Ease score and E-E-A-T signals (Author/Dates).

- **Forms:** Accessibility and security checks.

- **Media:** Video/Audio embed analysis.

- **Scoring System:** Weighted 0-100 grading with color-coded visual feedback.

**🏗 Architecture**

**The solution follows a clean separation of concerns:**

- **CanonicalSeoAuditor (Web):** ASP.NET Core MVC application serving the UI.

- **SeoAuditor.Crawler (Library):** A reusable C# class library containing the core SEO grading logic.

- **Node Crawler Service:** A lightweight Express.js microservice using Puppeteer to render pages and return raw HTML/DOM.

**🛠️ Tech Stack**

- **Frontend:** Razor Views, Bootstrap 5, Custom CSS/JS Animations.

- **Backend:** .NET 8, C#, HTMLAgilityPack.

- **Microservice:** Node.js, Express, Puppeteer (Headless Chrome).

- **Communication:** HTTP REST Client.

**⚙️ Prerequisites**

- .NET 8.0 SDK

- Node.js (v16 or higher)

**1. Setup the Node.js Crawler Service**

- This service is required to render webpages before analysis.

cd "SEO Auditor/NodeService" # Adjust path to where server.js is
npm install

- **Create a .env file in this directory:**

PORT=3000
CRAWLER_API_KEY=your_secure_dev_key_123

- **Start the service:**

node server.js

- **The service will listen on http://localhost:3000**

**2. Setup the .NET Web Application**

- **Navigate to the solution folder:**

cd "SEO Auditor"
dotnet restore

- **Update CanonicalSeoAuditor/appsettings.json to match your Node service configuration:**

"CrawlerSettings": {
  "BaseUrl": "http://localhost:3000",
  "ApiKey": "your_secure_dev_key_123",
  "Endpoint": "/render"
}

- **Run the application:**

dotnet run --project CanonicalSeoAuditor

**🖥️ Usage**

- Open your browser to https://localhost:7147 (or the port specified in your launch logs).

- Enter a full URL (e.g., https://www.example.com).

- Click Analyze.

- Wait for the crawler to render and analyze the page (approx. 5-10 seconds).

- Review your comprehensive score and detailed breakdown.

**🎨 Design**

**The UI utilizes a custom Palette featuring deep blues, ambers and clean typography (Syne & Outfit fonts) for a premium, professional feel. It includes:**

- Smooth scroll animations.

- Pulse loading states.

- Interactive score counters.

- Layman-friendly tooltips for all technical terms.

**🤝 Contributing**

- Contributions are welcome! Please fork the repository and create a pull request for any feature enhancements or bug fixes.

**📄 License**

This project is open-source and available under the MIT License.
