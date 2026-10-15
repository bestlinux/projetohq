using Microsoft.Extensions.Logging;
using ProjetoHQApi.Domain.Interfaces;
using PuppeteerSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class WebScraperService : IWebScraperService
    {
        private readonly ILogger<WebScraperService> _logger;
        private static string? _browserExecutablePath;
        private static readonly SemaphoreSlim _browserInitLock = new(1, 1);

        public WebScraperService(ILogger<WebScraperService> logger)
        {
            _logger = logger;
        }

        private async Task<string> ResolveBrowserExecutableAsync()
        {
            if (!string.IsNullOrEmpty(_browserExecutablePath) && File.Exists(_browserExecutablePath))
            {
                return _browserExecutablePath;
            }

            await _browserInitLock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_browserExecutablePath) && File.Exists(_browserExecutablePath))
                {
                    return _browserExecutablePath;
                }

                _logger.LogInformation("Checking / preparing browser for scraping via BrowserFetcher...");
                var browserFetcher = new BrowserFetcher();
                var installedBrowser = await browserFetcher.DownloadAsync();
                _browserExecutablePath = installedBrowser.GetExecutablePath();
                _logger.LogInformation("Browser executable ready at: {Path}", _browserExecutablePath);
                return _browserExecutablePath;
            }
            finally
            {
                _browserInitLock.Release();
            }
        }

        public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default)
        {
            var executablePath = await ResolveBrowserExecutableAsync();

            var tempProfileDir = Path.Combine(Path.GetTempPath(), "HQScraperProfile_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempProfileDir);

            var launchOptions = new LaunchOptions
            {
                Headless = false, // Headless false is required so Cloudflare Turnstile auto-resolves
                ExecutablePath = executablePath,
                UserDataDir = tempProfileDir,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-blink-features=AutomationControlled",
                    "--disable-infobars",
                    "--window-position=-2000,-2000", // Offscreen so no window interrupts the desktop
                    "--window-size=1920,1080"
                }
            };

            _logger.LogInformation("Launching browser to fetch URL: {Url}", url);

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

            try
            {
                await page.GoToAsync(url, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded },
                    Timeout = 40000
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Navigation warning: {Message}", ex.Message);
            }

            // Monitor up to 15 seconds for Cloudflare challenge completion
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 15000 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
                var title = await page.GetTitleAsync();

                if (!title.Contains("momento", StringComparison.OrdinalIgnoreCase) &&
                    !title.Contains("moment", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(title))
                {
                    _logger.LogInformation("Challenge resolved in {Elapsed}ms. Page Title: {Title}", stopwatch.ElapsedMilliseconds, title);
                    break;
                }
            }

            var html = await page.GetContentAsync();

            try
            {
                if (Directory.Exists(tempProfileDir))
                {
                    Directory.Delete(tempProfileDir, true);
                }
            }
            catch
            {
                // Ignore temp folder cleanup failure
            }

            return html;
        }
    }
}
