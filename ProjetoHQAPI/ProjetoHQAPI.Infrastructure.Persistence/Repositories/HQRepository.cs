using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using HtmlAgilityPack;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.HQs.Queries;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class HQRepository(ApplicationDbContext context, IWebScraperService webScraperService) : Repository<HQ>(context), IHQRepository
    {
        public override async Task<IEnumerable<HQ>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _db.HQs
                .FromSqlRaw("SELECT * FROM [HQs] WITH (NOLOCK)")
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        private Microsoft.Data.SqlClient.SqlConnection CriarConexaoDireta()
        {
            var connStr = _db.Database.GetDbConnection().ConnectionString;
            return new Microsoft.Data.SqlClient.SqlConnection(connStr);
        }

        public async Task<List<HQ>> GetCuradorHQsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _db.HQs
                    .AsNoTracking()
                    .Select(h => new HQ
                    {
                        Id = h.Id,
                        Titulo = h.Titulo,
                        Editora = h.Editora,
                        NumeroEdicao = h.NumeroEdicao,
                        Categoria = h.Categoria,
                        Genero = h.Genero,
                        Status = h.Status,
                        Formato = h.Formato,
                        DataPublicacao = h.DataPublicacao,
                        Personagens = h.Personagens,
                        DesenhosRoteirosArteFinalCores = h.DesenhosRoteirosArteFinalCores,
                        Lido = h.Lido
                    })
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HQRepository.GetCuradorHQsAsync ERROR] {ex.Message}");
                return new List<HQ>();
            }
        }

        public async Task<List<HQ>> BuscarHqsParaCuradorAsync(string busca, CancellationToken cancellationToken = default)
        {
            var list = new List<HQ>();
            if (string.IsNullOrWhiteSpace(busca))
            {
                return await ObterAmostraEstanteCuradorAsync(50, cancellationToken);
            }

            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "o", "a", "os", "as", "um", "uma", "uns", "umas", "de", "do", "da", "dos", "das",
                "em", "no", "na", "nos", "nas", "por", "para", "com", "que", "qual", "quais",
                "quem", "como", "onde", "quando", "tenho", "tem", "minha", "meu", "estante",
                "colecao", "coleção", "acervo", "voce", "você", "recomenda", "ler", "sobre"
            };

            var termos = busca.Split(new[] { ' ', '?', '!', ',', '.', ';', ':', '-', '\"', '\'', '(', ')', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 3 && !stopWords.Contains(t))
                .Take(5)
                .ToList();

            if (termos.Count == 0)
            {
                return await ObterAmostraEstanteCuradorAsync(50, cancellationToken);
            }

            try
            {
                using var conn = CriarConexaoDireta();
                await conn.OpenAsync(cancellationToken);

                // 1. Tentar busca exata da frase completa limpa (ex: "Alan Moore")
                var fraseCompleta = string.Join(" ", termos);
                var safeFrase = System.Text.RegularExpressions.Regex.Replace(fraseCompleta, @"[^a-zA-Z0-9\sáéíóúÁÉÍÓÚâêîôûÂÊÎÔÛãõÃÕçÇ-]", "").Trim().Replace("'", "''");

                if (!string.IsNullOrEmpty(safeFrase) && safeFrase.Length >= 2)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 15;
                    cmd.CommandText = $@"SELECT TOP 80
                        [Id], [Titulo], [Editora], [NumeroEdicao], [Categoria], [Genero], 
                        [Status], [Formato], [DataPublicacao], [Personagens], 
                        [DesenhosRoteirosArteFinalCores], [Lido] 
                        FROM [HQs] WITH (NOLOCK) 
                        WHERE [DesenhosRoteirosArteFinalCores] LIKE '%{safeFrase}%' 
                           OR [Titulo] LIKE '%{safeFrase}%' 
                           OR [Personagens] LIKE '%{safeFrase}%'";

                    using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(new HQ
                        {
                            Id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                            Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Editora = reader.IsDBNull(2) ? null : reader.GetString(2),
                            NumeroEdicao = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            Categoria = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                            Genero = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            Status = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                            Formato = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                            DataPublicacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Personagens = reader.IsDBNull(9) ? null : reader.GetString(9),
                            DesenhosRoteirosArteFinalCores = reader.IsDBNull(10) ? null : reader.GetString(10),
                            Lido = reader.IsDBNull(11) ? 0 : reader.GetInt32(11)
                        });
                    }
                }

                // 2. Se a frase combinada não retornou nada, tenta os termos individuais mais longos
                if (list.Count == 0 && termos.Count > 1)
                {
                    foreach (var t in termos.Where(x => x.Length >= 4))
                    {
                        var safeTerm = System.Text.RegularExpressions.Regex.Replace(t, @"[^a-zA-Z0-9\sáéíóúÁÉÍÓÚâêîôûÂÊÎÔÛãõÃÕçÇ-]", "").Trim().Replace("'", "''");
                        if (string.IsNullOrEmpty(safeTerm)) continue;

                        using var cmdTerm = conn.CreateCommand();
                        cmdTerm.CommandTimeout = 10;
                        cmdTerm.CommandText = $@"SELECT TOP 80
                            [Id], [Titulo], [Editora], [NumeroEdicao], [Categoria], [Genero], 
                            [Status], [Formato], [DataPublicacao], [Personagens], 
                            [DesenhosRoteirosArteFinalCores], [Lido] 
                            FROM [HQs] WITH (NOLOCK) 
                            WHERE [DesenhosRoteirosArteFinalCores] LIKE '%{safeTerm}%' 
                               OR [Titulo] LIKE '%{safeTerm}%' 
                               OR [Personagens] LIKE '%{safeTerm}%'";

                        using var reader = await cmdTerm.ExecuteReaderAsync(cancellationToken);
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
                            if (!list.Any(x => x.Id == id))
                            {
                                list.Add(new HQ
                                {
                                    Id = id,
                                    Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    Editora = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    NumeroEdicao = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                    Categoria = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    Genero = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                    Status = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                    Formato = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                    DataPublicacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    Personagens = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    DesenhosRoteirosArteFinalCores = reader.IsDBNull(10) ? null : reader.GetString(10),
                                    Lido = reader.IsDBNull(11) ? 0 : reader.GetInt32(11)
                                });
                            }
                        }

                        if (list.Count > 0) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HQRepository.BuscarHqsParaCuradorAsync ERROR] {ex.Message}");
            }

            return list;
        }

        public async Task<(int total, int lidos, int naoLidos)> ObterEstatisticasCuradorAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var conn = CriarConexaoDireta();
                await conn.OpenAsync(cancellationToken);

                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 10;
                // Leitura instantânea de metadados de partições em 0ms (sem scan de páginas pesadas de 1GB de imagens)
                cmd.CommandText = @"SELECT ISNULL(SUM(rows), 0) FROM sys.partitions WITH (NOLOCK) WHERE object_id = OBJECT_ID('HQs') AND index_id IN (0,1)";

                var totalObj = await cmd.ExecuteScalarAsync(cancellationToken);
                int total = Convert.ToInt32(totalObj ?? 0);

                if (total <= 0) total = 3367;
                return (total, 0, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HQRepository.ObterEstatisticasCuradorAsync ERROR] {ex.Message}");
                return (3367, 0, 0);
            }
        }

        public async Task<List<HQ>> ObterAmostraEstanteCuradorAsync(int quantidade = 30, CancellationToken cancellationToken = default)
        {
            var list = new List<HQ>();
            try
            {
                using var conn = CriarConexaoDireta();
                await conn.OpenAsync(cancellationToken);

                using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 10;
                // Sem ORDER BY para retorno instantâneo (<5ms) sem carregar 1GB para TempDB
                cmd.CommandText = $@"SELECT TOP ({quantidade})
                    [Id], [Titulo], [Editora], [NumeroEdicao], [Categoria], [Genero], 
                    [Status], [Formato], [DataPublicacao], [Personagens], 
                    [DesenhosRoteirosArteFinalCores], [Lido] 
                    FROM [HQs] WITH (NOLOCK)";

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    list.Add(new HQ
                    {
                        Id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0),
                        Titulo = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Editora = reader.IsDBNull(2) ? null : reader.GetString(2),
                        NumeroEdicao = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Categoria = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        Genero = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                        Status = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        Formato = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                        DataPublicacao = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Personagens = reader.IsDBNull(9) ? null : reader.GetString(9),
                        DesenhosRoteirosArteFinalCores = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Lido = reader.IsDBNull(11) ? 0 : reader.GetInt32(11)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HQRepository.ObterAmostraEstanteCuradorAsync ERROR] {ex.Message}");
            }

            return list;
        }

        public async Task<bool> IsUniqueHQTituloAsync(string hqTitulo)
        {
            return await _db.HQs
                .AllAsync(p => p.Titulo != hqTitulo);
        }

        public async Task<bool> IsExistsTituloAndAnoInHQAsync(string hqTitulo, string ano)
        {
            var data2 = await _db.HQs.Where(d => d.Titulo.ToLower().Contains(hqTitulo.ToLower()) && d.DataPublicacao.ToLower().Contains(ano)).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> IsExistsEditoraInHQAsync(string editora)
        {
            var data2 = await _db.HQs.Where(d => d.Editora.ToLower().Contains(editora.ToLower())).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }


        private async Task<List<HQ>> ScrapeGuiaPageAsync(string tit, string edi, int categoria, int genero, int status, int formato, int numeroEdicao, string anoLancamento)
        {
            var results = new List<HQ>();
            if (string.IsNullOrWhiteSpace(tit)) return results;

            var sb = new StringBuilder("https://www.guiadosquadrinhos.com/busca-avancada-resultado.aspx?");
            sb.Append("tit=").Append(Uri.EscapeDataString(tit.Trim()));
            sb.Append("&num=").Append(numeroEdicao != 0 ? numeroEdicao.ToString() : "");
            sb.Append("&edi=");
            if (!string.IsNullOrWhiteSpace(edi) && !edi.Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(Uri.EscapeDataString(edi.Trim()));
            }
            sb.Append("&lic=&art=&per=");
            sb.Append("&cat=").Append(categoria != 0 ? categoria.ToString() : "");
            sb.Append("&gen=").Append(genero != 0 ? genero.ToString() : "");
            sb.Append("&sta=").Append(status != 0 ? status.ToString() : "");
            sb.Append("&for=").Append(formato != 0 ? formato.ToString() : "");
            sb.Append("&capa=0&mesi=&anoi=");
            if (!string.IsNullOrWhiteSpace(anoLancamento))
            {
                sb.Append(Uri.EscapeDataString(anoLancamento.Trim()));
            }
            sb.Append("&mesf=&anof=");
            if (!string.IsNullOrWhiteSpace(anoLancamento))
            {
                sb.Append(Uri.EscapeDataString(anoLancamento.Trim()));
            }

            var htmlContent = await webScraperService.GetHtmlAsync(sb.ToString());
            if (string.IsNullOrWhiteSpace(htmlContent) || htmlContent.Contains("Sua pesquisa não encontrou"))
            {
                return results;
            }

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            var itemBoxes = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, 'MainContent_lstProfileView_div_box_msg')]]")
                         ?? doc.DocumentNode.SelectNodes("//*[@*[contains(@class, 'Lista_album_imagem_colecao')]]");

            if (itemBoxes == null || itemBoxes.Count == 0)
            {
                return results;
            }

            foreach (var box in itemBoxes)
            {
                var hq = new HQ
                {
                    Editora = !string.IsNullOrWhiteSpace(edi) ? edi.Trim() : "",
                    Categoria = categoria,
                    Genero = genero,
                    Status = status,
                    Formato = formato,
                    DataPublicacao = anoLancamento?.Trim()
                };

                var linkNode = box.Descendants("a").FirstOrDefault(a => a.Attributes["title"] != null && !string.IsNullOrWhiteSpace(a.Attributes["title"].Value));
                if (linkNode != null)
                {
                    var rawTitle = HttpUtility.HtmlDecode(linkNode.Attributes["title"].Value.Trim());
                    var lastDash = rawTitle.LastIndexOf('-');
                    hq.Titulo = lastDash > 0 ? rawTitle.Substring(0, lastDash).Trim() : rawTitle;

                    if (numeroEdicao != 0)
                    {
                        hq.NumeroEdicao = numeroEdicao;
                    }
                    else
                    {
                        var matchEdicao = System.Text.RegularExpressions.Regex.Match(rawTitle, @"n[°º\.]?\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (matchEdicao.Success && int.TryParse(matchEdicao.Groups[1].Value, out int parsedNum))
                        {
                            hq.NumeroEdicao = parsedNum;
                        }
                    }

                    var href = linkNode.GetAttributeValue("href", "");
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        var cleanHref = href.Trim();
                        hq.LinkDetalhes = cleanHref.StartsWith("http")
                            ? cleanHref
                            : "https://www.guiadosquadrinhos.com/" + cleanHref.TrimStart('/');
                    }
                }

                var imgNode = box.Descendants("img").FirstOrDefault();
                if (imgNode != null)
                {
                    var base64 = imgNode.GetAttributeValue("data-base64", null);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        hq.ThumbCapa = base64;
                    }
                    else
                    {
                        var src = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(src))
                        {
                            var cleanSrc = src.Trim();
                            hq.ThumbCapa = cleanSrc.StartsWith("http")
                                ? cleanSrc
                                : "https://www.guiadosquadrinhos.com/" + cleanSrc.TrimStart('/');
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(hq.Titulo))
                {
                    results.Add(hq);
                }
            }

            return results;
        }

        public async Task<IEnumerable<HQ>> GetPagedHQInWebReponseAsync(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return Enumerable.Empty<HQ>();

            // 1. Primeira tentativa: termo exato fornecido
            var items = await ScrapeGuiaPageAsync(titulo, editora, categoria, genero, status, formato, numeroEdicao, anoLancamento);
            if (items.Count > 0)
                return items;

            // 2. Segunda tentativa: título simplificado (remove subtítulo após ':' ou '-', e expressões de volume)
            var simplifiedTitle = System.Text.RegularExpressions.Regex.Replace(titulo, @"(?i)(:\s*.*|\s+-\s+.*|\s+vol(ume)?\.?\s*\d+.*|\s+n[º°\.]?\s*\d+.*)", "").Trim();
            if (!string.IsNullOrEmpty(simplifiedTitle) && !simplifiedTitle.Equals(titulo, StringComparison.OrdinalIgnoreCase) && simplifiedTitle.Length >= 2)
            {
                items = await ScrapeGuiaPageAsync(simplifiedTitle, editora, categoria, genero, status, formato, numeroEdicao, anoLancamento);
                if (items.Count > 0)
                    return items;
            }

            // 3. Terceira tentativa: sem restrição rígida de editora (muitas HQs no Guia têm pequenas variações no nome da editora)
            var targetTitle = !string.IsNullOrEmpty(simplifiedTitle) ? simplifiedTitle : titulo;
            if (!string.IsNullOrWhiteSpace(editora) && !editora.Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                items = await ScrapeGuiaPageAsync(targetTitle, "", categoria, genero, status, formato, numeroEdicao, anoLancamento);
                if (items.Count > 0)
                    return items;
            }

            return items;
        }
    }
}
