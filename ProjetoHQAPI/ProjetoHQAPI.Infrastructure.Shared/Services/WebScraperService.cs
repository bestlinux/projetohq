using Microsoft.Extensions.Logging;
using ProjetoHQApi.Domain.Interfaces;
using PuppeteerSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static readonly SemaphoreSlim _scrapeLock = new(1, 1);
        private static readonly ConcurrentDictionary<string, (string Html, DateTime CachedAt)> _cache = new();

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

                // 1. Prioriza o Google Chrome ou Edge nativos instalados no Windows (máxima reputação de bot e Turnstile do Cloudflare)
                string[] standardPaths = {
                    @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                    @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
                };

                foreach (var path in standardPaths)
                {
                    if (File.Exists(path))
                    {
                        _browserExecutablePath = path;
                        _logger.LogInformation("Usando navegador instalado nativo para scraping anti-bloqueio: {Path}", _browserExecutablePath);
                        return _browserExecutablePath;
                    }
                }

                _logger.LogInformation("Nenhum navegador nativo encontrado. Baixando via BrowserFetcher...");
                var browserFetcher = new BrowserFetcher();
                var installedBrowser = await browserFetcher.DownloadAsync();
                _browserExecutablePath = installedBrowser.GetExecutablePath();
                _logger.LogInformation("Navegador baixado pronto em: {Path}", _browserExecutablePath);
                return _browserExecutablePath;
            }
            finally
            {
                _browserInitLock.Release();
            }
        }

        public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default)
        {
            // 1. Cache em memória de 10 minutos para evitar chamadas idênticas repetidas
            if (_cache.TryGetValue(url, out var cached) && (DateTime.UtcNow - cached.CachedAt).TotalMinutes < 10)
            {
                _logger.LogInformation("Retornando resultado do Guia dos Quadrinhos em cache para URL: {Url}", url);
                return cached.Html;
            }

            var executablePath = await ResolveBrowserExecutableAsync();

            // 2. Perfil persistente no AppData: salva cookies e clearance do Cloudflare para requisições futuras
            var baseAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var persistentProfileDir = Path.Combine(baseAppData, "ProjetoHQ", "ScraperProfile");
            Directory.CreateDirectory(persistentProfileDir);

            var launchOptions = new LaunchOptions
            {
                Headless = false, // Modo visível/offscreen permite ao Cloudflare validar o ambiente real do Chrome
                ExecutablePath = executablePath,
                UserDataDir = persistentProfileDir,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-blink-features=AutomationControlled",
                    "--disable-infobars",
                    "--window-position=-2500,-2500", // Fora do campo de visão da tela para não incomodar o usuário
                    "--window-size=1920,1080",
                    "--lang=pt-BR,pt"
                }
            };

            // Fila sequencial para garantir que duas requisições simultâneas não colidam no perfil
            await _scrapeLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Iniciando navegador com perfil persistente para URL: {Url}", url);

                await using var browser = await Puppeteer.LaunchAsync(launchOptions);
                await using var page = await browser.NewPageAsync();

                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

                // User-Agent real de Chrome moderno no Windows
                const string realUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
                await page.SetUserAgentAsync(realUserAgent);

                // Cabeçalhos HTTP realistas
                await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
                {
                    { "Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7" },
                    { "Sec-Ch-Ua", "\"Chromium\";v=\"131\", \"Google Chrome\";v=\"131\", \"Not_A Brand\";v=\"24\"" },
                    { "Sec-Ch-Ua-Mobile", "?0" },
                    { "Sec-Ch-Ua-Platform", "\"Windows\"" }
                });

                // Mascara identificadores de automação do Puppeteer (navigator.webdriver = false, plugins, chrome object)
                await page.EvaluateFunctionOnNewDocumentAsync(@"() => {
                    Object.defineProperty(navigator, 'webdriver', {
                        get: () => undefined
                    });
                    window.chrome = {
                        runtime: {}
                    };
                    Object.defineProperty(navigator, 'plugins', {
                        get: () => [1, 2, 3, 4, 5]
                    });
                    Object.defineProperty(navigator, 'languages', {
                        get: () => ['pt-BR', 'pt', 'en-US', 'en']
                    });
                }");

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
                    _logger.LogWarning("GoToAsync direto falhou ({Message}). Tentando via location.href...", ex.Message);
                    try
                    {
                        await page.EvaluateFunctionAsync("u => { window.location.href = u; }", url);
                        await page.WaitForNavigationAsync(new NavigationOptions
                        {
                            WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded },
                            Timeout = 30000
                        });
                    }
                    catch (Exception navEx)
                    {
                        _logger.LogWarning("Fallback de navegação: {Message}", navEx.Message);
                    }
                }

                // Monitora a resolução do desafio Cloudflare Turnstile por até 15 segundos
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < 15000 && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                    var currentUrl = page.Url;
                    var title = await page.GetTitleAsync();

                    // Se ainda estiver em about:blank, tenta forçar href
                    if (currentUrl == "about:blank")
                    {
                        try
                        {
                            await page.EvaluateFunctionAsync("u => { window.location.href = u; }", url);
                        }
                        catch { }
                    }

                    if (!title.Contains("momento", StringComparison.OrdinalIgnoreCase) &&
                        !title.Contains("moment", StringComparison.OrdinalIgnoreCase) &&
                        !title.Contains("cloudflare", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(title) &&
                        !title.Equals("about:blank", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Desafio resolvido em {Elapsed}ms. Título: {Title}", stopwatch.ElapsedMilliseconds, title);
                        break;
                    }

                    // Tenta clicar na checkbox do Turnstile caso esteja visível em iframe
                    try
                    {
                        var frames = page.Frames;
                        foreach (var frame in frames)
                        {
                            var box = await frame.QuerySelectorAsync("input[type=checkbox], #challenge-stage input, .ctp-checkbox-label");
                            if (box != null)
                            {
                                await box.ClickAsync();
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // Continua aguardando resolução automática
                    }
                }

                // Aguarda 1 segundo adicional para renderização das capas
                await Task.Delay(1000, cancellationToken);

                // Converte imagens renderizadas em base64 caso disponíveis
                try
                {
                    await page.EvaluateFunctionAsync(@"() => {
                        const images = document.querySelectorAll('img');
                        images.forEach(img => {
                            try {
                                if (!img.src || img.src.startsWith('data:')) return;
                                const canvas = document.createElement('canvas');
                                canvas.width = img.naturalWidth || img.width || 100;
                                canvas.height = img.naturalHeight || img.height || 150;
                                const ctx = canvas.getContext('2d');
                                ctx.drawImage(img, 0, 0);
                                const dataUrl = canvas.toDataURL('image/jpeg', 0.85);
                                if (dataUrl && dataUrl.length > 500) {
                                    img.setAttribute('data-base64', dataUrl);
                                }
                            } catch (e) {
                                // ignora erros em imagens cross-origin
                            }
                        });
                    }");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Não foi possível extrair base64 das imagens: {Message}", ex.Message);
                }

                var html = await page.GetContentAsync();

                // Armazena no cache recente
                _cache[url] = (html, DateTime.UtcNow);

                return html;
            }
            finally
            {
                _scrapeLock.Release();
            }
        }
    }
}
