using Microsoft.Extensions.Logging;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class JevEngineService : IJevEngineService
    {
        // Dicionário canônico de editoras para harmonização
        private static readonly Dictionary<string, string> EditorasCanonicas = new(StringComparer.OrdinalIgnoreCase)
        {
            { "panini comics", "Panini" },
            { "panini brasil", "Panini" },
            { "editora panini", "Panini" },
            { "panini books", "Panini" },
            { "panini", "Panini" },
            { "editora abril", "Abril" },
            { "abril jovem", "Abril" },
            { "abril", "Abril" },
            { "dc comics", "DC" },
            { "dc", "DC" },
            { "marvel comics", "Marvel" },
            { "marvel", "Marvel" },
            { "mythos editora", "Mythos" },
            { "mythos", "Mythos" },
            { "editora globo", "Globo" },
            { "globo", "Globo" },
            { "pipoca & nanquim", "Pipoca & Nanquim" },
            { "pipoca e nanquim", "Pipoca & Nanquim" },
            { "pipoca nanquim", "Pipoca & Nanquim" },
            { "darkside books", "Darkside" },
            { "darkside", "Darkside" },
            { "editora jbc", "JBC" },
            { "jbc", "JBC" },
            { "editora veneta", "Veneta" },
            { "veneta", "Veneta" },
            { "novo seculo", "Novo Século" },
            { "salvat", "Salvat" },
            { "eaglemoss", "Eaglemoss" },
            { "devir", "Devir" },
            { "devir livraria", "Devir" },
            { "record", "Record" }
        };

        // Ruídos e sufixos de títulos para remoção na comparação fonética/normalizada
        private static readonly string[] RuidosTitulo = new[]
        {
            "edicao definitiva", "edicao de luxo", "deluxe edition", "edicao especial",
            "capa dura", "omnibus", "graphic novel", "volume", "vol.", "vol", "edicao", "ed.", "ed"
        };

        private readonly IHQRepository _hqRepository;
        private readonly ITelemetriaIaService _telemetriaService;
        private readonly ILogger<JevEngineService> _logger;

        private static List<HQ>? _cachedHqs;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly SemaphoreSlim _cacheLock = new(1, 1);

        public JevEngineService(
            IHQRepository hqRepository,
            ITelemetriaIaService telemetriaService,
            ILogger<JevEngineService> logger)
        {
            _hqRepository = hqRepository;
            _telemetriaService = telemetriaService;
            _logger = logger;
        }

        private async Task<List<HQ>> ObterHqsCacheAsync(CancellationToken ct)
        {
            if (_cachedHqs != null && _cachedHqs.Count > 0 && (DateTime.UtcNow - _lastCacheTime).TotalMinutes < 30)
            {
                return _cachedHqs;
            }

            await _cacheLock.WaitAsync(ct);
            try
            {
                if (_cachedHqs != null && _cachedHqs.Count > 0 && (DateTime.UtcNow - _lastCacheTime).TotalMinutes < 30)
                {
                    return _cachedHqs;
                }

                var list = await _hqRepository.GetCuradorHQsAsync(ct);
                if (list != null && list.Count > 0)
                {
                    _cachedHqs = list;
                    _lastCacheTime = DateTime.UtcNow;
                    return list;
                }
                return _cachedHqs ?? new List<HQ>();
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        #region 1. Similaridade Probabilística (JEV Deduplication Engine)

        public JevProbabilisticMatchResult CalcularSimilaridade(
            string tituloA, string? editoraA, int edicaoA,
            string tituloB, string? editoraB, int edicaoB)
        {
            var normTitA = NormalizarTituloJev(tituloA);
            var normTitB = NormalizarTituloJev(tituloB);

            if (string.IsNullOrWhiteSpace(normTitA) || string.IsNullOrWhiteSpace(normTitB))
            {
                return new JevProbabilisticMatchResult
                {
                    ScoreGeral = 0,
                    Classificacao = "Distinto",
                    Motivo = "Título vazio ou inválido."
                };
            }

            // 1. Métricas de Título (Token Set Ratio 50% + Jaro-Winkler 30% + Levenshtein 20%)
            double levRatio = CalcularLevenshteinRatio(normTitA, normTitB);
            double jaroWinkler = CalcularJaroWinkler(normTitA, normTitB);
            double tokenSetRatio = CalcularTokenSetRatio(normTitA, normTitB);

            double scoreTitulo = (tokenSetRatio * 0.50) + (jaroWinkler * 0.30) + (levRatio * 0.20);
            scoreTitulo = Math.Clamp(scoreTitulo * 100.0, 0.0, 100.0);

            // 2. Métrica de Edição
            int numA = edicaoA <= 0 ? 1 : edicaoA;
            int numB = edicaoB <= 0 ? 1 : edicaoB;
            double scoreEdicao;
            if (numA == numB)
            {
                scoreEdicao = 100.0;
            }
            else
            {
                scoreEdicao = 0.0;
            }

            // 3. Métrica de Editora
            var canEdA = NormalizarEditoraCanonico(editoraA);
            var canEdB = NormalizarEditoraCanonico(editoraB);
            double scoreEditora;

            if (string.IsNullOrWhiteSpace(canEdA) || string.IsNullOrWhiteSpace(canEdB))
            {
                scoreEditora = 60.0; // Neutro se uma não informou
            }
            else if (string.Equals(canEdA, canEdB, StringComparison.OrdinalIgnoreCase))
            {
                scoreEditora = 100.0;
            }
            else if (canEdA.Contains(canEdB, StringComparison.OrdinalIgnoreCase) || canEdB.Contains(canEdA, StringComparison.OrdinalIgnoreCase))
            {
                scoreEditora = 85.0;
            }
            else
            {
                scoreEditora = 15.0;
            }

            // 4. Score Composto Probabilístico
            // Pesos: 55% Título + 25% Edição + 20% Editora
            double scoreGeral = (scoreTitulo * 0.55) + (scoreEdicao * 0.25) + (scoreEditora * 0.20);

            // Regra de Ouro JEV: Se o título tem equivalência idêntica (>=98%) e a edição bate, é duplicata garantida
            if (scoreTitulo >= 98.0 && scoreEdicao == 100.0)
            {
                scoreGeral = Math.Max(scoreGeral, 97.0);
            }

            // Classificação Semântica
            string classificacao;
            string motivo;

            if (scoreGeral >= 88.0)
            {
                classificacao = "Duplicata Confirmada";
                motivo = $"Alta probabilidade de duplicata ({Math.Round(scoreGeral)}%). Título e edição idênticos ou variantes da mesma editora.";
            }
            else if (scoreTitulo >= 80.0 && scoreEdicao == 0.0)
            {
                classificacao = "Mesma Série (Outra Edição)";
                motivo = $"Pertence à mesma série/coleção ({Math.Round(scoreTitulo)}% de similaridade no título), porém edição #{numB} vs #{numA}.";
            }
            else if (scoreGeral >= 68.0 || (scoreTitulo >= 75.0 && scoreEditora >= 60.0))
            {
                classificacao = "Possível Variante / Reimpressão";
                motivo = $"Forte correlação ({Math.Round(scoreGeral)}%). Pode ser reimpressão, edição especial ou encadernado correlato.";
            }
            else if (scoreGeral >= 48.0)
            {
                classificacao = "Semelhante";
                motivo = $"Títulos ou editoras com palavras semelhantes ({Math.Round(scoreGeral)}%).";
            }
            else
            {
                classificacao = "Distinto";
                motivo = "Baixa probabilidade de correlação.";
            }

            return new JevProbabilisticMatchResult
            {
                ScoreGeral = Math.Round(scoreGeral, 1),
                ScoreTitulo = Math.Round(scoreTitulo, 1),
                ScoreEditora = Math.Round(scoreEditora, 1),
                ScoreEdicao = Math.Round(scoreEdicao, 1),
                Classificacao = classificacao,
                Motivo = motivo
            };
        }

        public async Task<List<JevColecaoMatchDto>> BuscarSemelhantesColecaoAsync(
            string titulo, string? editora, int edicao, int maxResultados = 30, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            if (string.IsNullOrWhiteSpace(titulo)) return new List<JevColecaoMatchDto>();

            var todasHqs = await ObterHqsCacheAsync(ct);
            if (todasHqs == null || todasHqs.Count == 0) return new List<JevColecaoMatchDto>();

            var normBusca = NormalizarTituloJev(titulo);
            var tokensBusca = new HashSet<string>(normBusca.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length > 2));

            var matches = new ConcurrentBag<JevColecaoMatchDto>();

            // Filtragem rápida em paralelo (Processamento multi-core System 1 em <10ms para 3.500 itens)
            Parallel.ForEach(todasHqs, hq =>
            {
                var normHq = NormalizarTituloJev(hq.Titulo);
                if (string.IsNullOrWhiteSpace(normHq)) return;

                // Pré-filtro rápido (at least 1 common token or substring match or length check)
                bool candidato = normBusca.Contains(normHq) || normHq.Contains(normBusca);
                if (!candidato && tokensBusca.Count > 0)
                {
                    var tokensHq = normHq.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tok in tokensHq)
                    {
                        if (tokensBusca.Contains(tok))
                        {
                            candidato = true;
                            break;
                        }
                    }
                }

                if (!candidato && Math.Abs(normBusca.Length - normHq.Length) > 15)
                {
                    return;
                }

                var res = CalcularSimilaridade(titulo, editora, edicao, hq.Titulo ?? string.Empty, hq.Editora, hq.NumeroEdicao);
                if (res.ScoreGeral >= 45.0)
                {
                    matches.Add(new JevColecaoMatchDto
                    {
                        Id = hq.Id,
                        Titulo = hq.Titulo ?? string.Empty,
                        Editora = hq.Editora ?? string.Empty,
                        NumeroEdicao = hq.NumeroEdicao,
                        Lido = hq.Lido,
                        Imagem = hq.ThumbCapa ?? hq.Capa,
                        ScoreGeral = res.ScoreGeral,
                        ScoreTitulo = res.ScoreTitulo,
                        ScoreEditora = res.ScoreEditora,
                        ScoreEdicao = res.ScoreEdicao,
                        Classificacao = res.Classificacao,
                        Motivo = res.Motivo
                    });
                }
            });

            sw.Stop();
            _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
            {
                Funcionalidade = "JEV_Deduplicacao",
                Operacao = "deduplicacao.probabilistica",
                Modelo = "jev-system1-local",
                OrigemMotor = "JEV_SYSTEM1_LOCAL",
                PromptTokens = 0,
                CompletionTokens = 0,
                QtdFerramentas = 1,
                LatenciaMs = sw.ElapsedMilliseconds,
                Sucesso = true,
                CodigoStatus = 200
            }, ct);

            return matches
                .OrderByDescending(m => m.ScoreGeral)
                .ThenByDescending(m => m.ScoreTitulo)
                .Take(maxResultados)
                .ToList();
        }

        #endregion

        #region 2. Roteador de Intenções Rápidas (System 1 Dispatcher)

        public async Task<ComandoCrudPreviewResponse?> RotearComandoCrudSystem1Async(
            string comando, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(comando)) return null;

            var sw = Stopwatch.StartNew();
            var cmdTrim = comando.Trim();

            // Padrão 1: Marcar como lido / já li
            // Ex: "marcar Watchmen como lido", "li Batman #1", "marque Sandman 2 como lida", "marcar como lida a hq X"
            var matchMarcarLido = Regex.Match(
                cmdTrim,
                @"^(?:já\s+li|li|marcar?\s+(?:como\s+)?lida?o?s?|marque\s+(?:como\s+)?lida?o?s?|defini?r?\s+como\s+lida?o?s?)\s*(?:a\s+hq\s+|o\s+quadrinho\s+)?(?<termo>.+?)(?:\s+#?(?<edicao>\d+))?$",
                RegexOptions.IgnoreCase);

            if (!matchMarcarLido.Success)
            {
                // Inversão: "marcar [HQ] como lido"
                matchMarcarLido = Regex.Match(
                    cmdTrim,
                    @"^(?:marcar?|marque|coloque?)\s+(?:a\s+hq\s+|o\s+quadrinho\s+)?(?<termo>.+?)\s+(?:como\s+)?(?:lida?o?s?)(?:\s+#?(?<edicao>\d+))?$",
                    RegexOptions.IgnoreCase);
            }

            if (matchMarcarLido.Success)
            {
                var termo = matchMarcarLido.Groups["termo"].Value.Trim();
                int edicao = 1;
                if (matchMarcarLido.Groups["edicao"].Success && int.TryParse(matchMarcarLido.Groups["edicao"].Value, out int ed))
                    edicao = ed;

                var bestMatch = await EncontrarMelhorMatchAsync(termo, edicao, ct);
                if (bestMatch != null)
                {
                    sw.Stop();
                    return RetornarComandoJev(new ComandoCrudPreviewResponse
                    {
                        ComandoOriginal = comando,
                        Acao = "marcar_lido",
                        EntidadeAlvo = "HQ",
                        HqIdAlvo = bestMatch.Id,
                        TituloAlvo = bestMatch.Titulo,
                        EditoraAlvo = bestMatch.Editora,
                        EdicaoAlvo = bestMatch.NumeroEdicao,
                        InterpretacaoHumana = $"⚡ [JEV Engine] Marcar '{bestMatch.Titulo}' #{bestMatch.NumeroEdicao} ({bestMatch.Editora}) como lido.",
                        ParametrosModificadosJson = "{\"Lido\": 1}",
                        RequerConfirmacao = true,
                        OrigemMotor = "JEV_SYSTEM1_LOCAL"
                    }, sw, ct);
                }
            }

            // Padrão 2: Desmarcar como lido / não li
            var matchDesmarcarLido = Regex.Match(
                cmdTrim,
                @"^(?:não\s+li|nao\s+li|desmarcar?\s+(?:como\s+)?lida?o?s?|desmarque\s+(?:como\s+)?lida?o?s?)\s*(?:a\s+hq\s+)?(?<termo>.+?)(?:\s+#?(?<edicao>\d+))?$",
                RegexOptions.IgnoreCase);

            if (!matchDesmarcarLido.Success)
            {
                matchDesmarcarLido = Regex.Match(
                    cmdTrim,
                    @"^(?:marcar?|marque)\s+(?<termo>.+?)\s+como\s+(?:não|nao)\s+lida?o?s?(?:\s+#?(?<edicao>\d+))?$",
                    RegexOptions.IgnoreCase);
            }

            if (matchDesmarcarLido.Success)
            {
                var termo = matchDesmarcarLido.Groups["termo"].Value.Trim();
                int edicao = 1;
                if (matchDesmarcarLido.Groups["edicao"].Success && int.TryParse(matchDesmarcarLido.Groups["edicao"].Value, out int ed))
                    edicao = ed;

                var bestMatch = await EncontrarMelhorMatchAsync(termo, edicao, ct);
                if (bestMatch != null)
                {
                    return RetornarComandoJev(new ComandoCrudPreviewResponse
                    {
                        ComandoOriginal = comando,
                        Acao = "desmarcar_lido",
                        EntidadeAlvo = "HQ",
                        HqIdAlvo = bestMatch.Id,
                        TituloAlvo = bestMatch.Titulo,
                        EditoraAlvo = bestMatch.Editora,
                        EdicaoAlvo = bestMatch.NumeroEdicao,
                        InterpretacaoHumana = $"⚡ [JEV Engine] Desmarcar '{bestMatch.Titulo}' #{bestMatch.NumeroEdicao} como lido (mover para fila de leitura).",
                        ParametrosModificadosJson = "{\"Lido\": 0}",
                        RequerConfirmacao = true,
                        OrigemMotor = "JEV_SYSTEM1_LOCAL"
                    }, sw, ct);
                }
            }

            // Padrão 3: Excluir / Remover HQ
            var matchExcluir = Regex.Match(
                cmdTrim,
                @"^(?:excluir?|exclua|remover?|remova|deletar?|delete)\s+(?:a\s+hq\s+|o\s+quadrinho\s+)?(?<termo>.+?)(?:\s+da\s+(?:minha\s+)?coleção|\s+da\s+estante)?(?:\s+#?(?<edicao>\d+))?$",
                RegexOptions.IgnoreCase);

            if (matchExcluir.Success)
            {
                var termo = matchExcluir.Groups["termo"].Value.Trim();
                int edicao = 1;
                if (matchExcluir.Groups["edicao"].Success && int.TryParse(matchExcluir.Groups["edicao"].Value, out int ed))
                    edicao = ed;

                var bestMatch = await EncontrarMelhorMatchAsync(termo, edicao, ct);
                if (bestMatch != null)
                {
                    return RetornarComandoJev(new ComandoCrudPreviewResponse
                    {
                        ComandoOriginal = comando,
                        Acao = "excluir_hq",
                        EntidadeAlvo = "HQ",
                        HqIdAlvo = bestMatch.Id,
                        TituloAlvo = bestMatch.Titulo,
                        EditoraAlvo = bestMatch.Editora,
                        EdicaoAlvo = bestMatch.NumeroEdicao,
                        InterpretacaoHumana = $"⚡ [JEV Engine] Remover '{bestMatch.Titulo}' #{bestMatch.NumeroEdicao} permanentemente da estante.",
                        ParametrosModificadosJson = "{\"Acao\": \"Exclusão\"}",
                        RequerConfirmacao = true,
                        OrigemMotor = "JEV_SYSTEM1_LOCAL"
                    }, sw, ct);
                }
            }

            // Padrão 4: Criar Desejo
            var matchDesejo = Regex.Match(
                cmdTrim,
                @"^(?:quero\s+comprar|adicionar?\s+(?:à|a|na)\s+lista\s+de\s+desejos?|colocar?\s+(?:na\s+)?lista\s+de\s+desejos?)\s*(?:a\s+hq\s+)?(?<termo>.+?)(?:\s+#?(?<edicao>\d+))?$",
                RegexOptions.IgnoreCase);

            if (matchDesejo.Success)
            {
                var termo = matchDesejo.Groups["termo"].Value.Trim();
                int edicao = 1;
                if (matchDesejo.Groups["edicao"].Success && int.TryParse(matchDesejo.Groups["edicao"].Value, out int ed))
                    edicao = ed;

                return RetornarComandoJev(new ComandoCrudPreviewResponse
                {
                    ComandoOriginal = comando,
                    Acao = "criar_desejo",
                    EntidadeAlvo = "Desejo",
                    TituloAlvo = termo,
                    EditoraAlvo = "Panini",
                    EdicaoAlvo = edicao,
                    InterpretacaoHumana = $"⚡ [JEV Engine] Adicionar '{termo}' #{edicao} à sua Lista de Desejos.",
                    ParametrosModificadosJson = "{\"Destino\": \"ListaDeDesejos\"}",
                    RequerConfirmacao = true,
                    OrigemMotor = "JEV_SYSTEM1_LOCAL"
                }, sw, ct);
            }

            // Se não coincidir com nenhum padrão determinístico local, retorna null para delegar ao Sistema 2 (Gemini)
            return null;
        }

        private ComandoCrudPreviewResponse RetornarComandoJev(ComandoCrudPreviewResponse preview, Stopwatch sw, CancellationToken ct)
        {
            sw.Stop();
            preview.TempoProcessamentoMs = sw.ElapsedMilliseconds;
            _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
            {
                Funcionalidade = "Comando_Crud",
                Operacao = "crud.preview.system1",
                Modelo = "jev-system1-local",
                OrigemMotor = "JEV_SYSTEM1_LOCAL",
                PromptTokens = 0,
                CompletionTokens = 0,
                QtdFerramentas = 1,
                LatenciaMs = sw.ElapsedMilliseconds,
                Sucesso = true,
                CodigoStatus = 200
            }, ct);
            return preview;
        }

        public async Task<ChatbotResponse?> RotearChatbotSystem1Async(
            ChatbotRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request?.Mensagem)) return null;

            var msg = request.Mensagem.Trim().ToLowerInvariant();
            var sw = Stopwatch.StartNew();

            // 1. Estatísticas gerais da coleção
            // Ex: "quantas hqs eu tenho?", "qual o total de hqs?", "quantos quadrinhos?", "estatísticas da coleção"
            if (msg.Contains("quantas hq") || msg.Contains("total de hq") || msg.Contains("tamanho da cole") || 
                msg.Contains("quantos quadrinhos") || msg == "estatisticas" || msg == "estatísticas")
            {
                var stats = await _hqRepository.ObterEstatisticasCuradorAsync(ct);
                double porcLidos = stats.total > 0 ? (stats.lidos * 100.0 / stats.total) : 0;
                double porcNaoLidos = stats.total > 0 ? (stats.naoLidos * 100.0 / stats.total) : 0;

                return RetornarChatbotJev(new ChatbotResponse
                {
                    Resposta = $@"⚡ **[JEV Engine • Sistema 1 Local — {sw.ElapsedMilliseconds}ms]**

Sua coleção conta atualmente com um acervo impressionante de **{stats.total:N0} quadrinhos** cadastrados:

- 🟢 **Lidos:** {stats.lidos:N0} edições ({porcLidos:F1}%)
- 🟡 **Fila de Leitura:** {stats.naoLidos:N0} edições ({porcNaoLidos:F1}%)

> *Dica do Curador:* Você já concluiu **{porcLidos:F1}%** do seu acervo. Se precisar de uma recomendação da sua fila de leitura, basta pedir!",
                    HqsMencionadas = new List<string>(),
                    SugestoesPerguntas = new List<string>
                    {
                        "O que você recomenda eu ler da minha fila?",
                        "Quais são minhas séries incompletas?",
                        "O que eu tenho do Alan Moore?"
                    },
                    OrigemMotor = "JEV_SYSTEM1_LOCAL"
                }, sw, ct);
            }

            // 2. Consulta de HQs por Autor / Personagem específico
            // Ex: "o que eu tenho do Alan Moore?", "tenho Batman?", "quais hqs do Demolidor eu tenho?"
            var matchAutorOuPersonagem = Regex.Match(
                request.Mensagem.Trim(),
                @"^(?:o\s+que\s+(?:eu\s+)?tenho\s+(?:d[oe]|da)\s+|quais\s+(?:hqs|quadrinhos)\s+(?:d[oe]|da)\s+|tenho\s+alguma\s+coisa\s+(?:d[oe]|da)\s+|tenho\s+)(?<termo>[^?]+)\??$",
                RegexOptions.IgnoreCase);

            if (matchAutorOuPersonagem.Success)
            {
                var termo = matchAutorOuPersonagem.Groups["termo"].Value.Trim();
                if (termo.Length >= 3 && !termo.Contains("recomenda", StringComparison.OrdinalIgnoreCase))
                {
                    var hqs = await _hqRepository.BuscarHqsParaCuradorAsync(termo, ct);
                    if (hqs != null && hqs.Count > 0)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"⚡ **[JEV Engine • Sistema 1 Local — {sw.ElapsedMilliseconds}ms]**");
                        sb.AppendLine();
                        sb.AppendLine($"Encontrei **{hqs.Count} edição(ões)** no seu acervo relacionadas a **{termo}**:");
                        sb.AppendLine();

                        foreach (var h in hqs.Take(15))
                        {
                            var badgeLido = h.Lido == 1 ? "🟢 *Lido*" : "🟡 *Não lido*";
                            sb.AppendLine($"- **{h.Titulo}** #{h.NumeroEdicao} ({h.Editora}) — {badgeLido}");
                        }

                        if (hqs.Count > 15)
                        {
                            sb.AppendLine();
                            sb.AppendLine($"*(...e mais {hqs.Count - 15} edições registradas na estante)*");
                        }

                        var titulos = hqs.Take(5).Select(h => h.Titulo ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

                        return RetornarChatbotJev(new ChatbotResponse
                        {
                            Resposta = sb.ToString(),
                            HqsMencionadas = titulos,
                            SugestoesPerguntas = new List<string>
                            {
                                $"Qual a ordem ideal de leitura de {termo}?",
                                $"Crie um recap narrativo de {titulos.FirstOrDefault() ?? termo}",
                                "O que mais falta para completar essa coleção?"
                            },
                            OrigemMotor = "JEV_SYSTEM1_LOCAL"
                        }, sw, ct);
                    }
                }
            }

            // Não é uma intenção rápida ou determinística -> Escala para o Sistema 2 (Gemini)
            return null;
        }

        private ChatbotResponse RetornarChatbotJev(ChatbotResponse resposta, Stopwatch sw, CancellationToken ct)
        {
            sw.Stop();
            resposta.TempoProcessamentoMs = sw.ElapsedMilliseconds;
            _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
            {
                Funcionalidade = "Curador_Chatbot",
                Operacao = "intent.system1.local",
                Modelo = "jev-system1-local",
                OrigemMotor = "JEV_SYSTEM1_LOCAL",
                PromptTokens = 0,
                CompletionTokens = 0,
                QtdFerramentas = 1,
                LatenciaMs = sw.ElapsedMilliseconds,
                Sucesso = true,
                CodigoStatus = 200
            }, ct);
            return resposta;
        }

        private async Task<HQ?> EncontrarMelhorMatchAsync(string termo, int edicao, CancellationToken ct)
        {
            var hqs = await _hqRepository.BuscarHqsParaCuradorAsync(termo, ct);
            if (hqs == null || hqs.Count == 0)
            {
                var amostra = await _hqRepository.ObterAmostraEstanteCuradorAsync(100, ct);
                hqs = amostra;
            }

            HQ? melhor = null;
            double maiorScore = 0.0;

            foreach (var h in hqs)
            {
                var res = CalcularSimilaridade(termo, null, edicao, h.Titulo ?? string.Empty, h.Editora, h.NumeroEdicao);
                if (res.ScoreGeral > maiorScore && res.ScoreGeral >= 65.0)
                {
                    maiorScore = res.ScoreGeral;
                    melhor = h;
                }
            }

            return melhor;
        }

        #endregion

        #region 3. Funções Matemáticas de Strings (Levenshtein, Jaro-Winkler, Token Set)

        public static string NormalizarTituloJev(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Remove acentos
            string textoNormalizado = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in textoNormalizado)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string limpo = sb.ToString().ToLowerInvariant();

            // Remove pontuação comum mantendo letras e dígitos
            limpo = Regex.Replace(limpo, @"[^\w\s]", " ");

            // Remove ruídos típicos de quadrinhos
            foreach (var ruido in RuidosTitulo)
            {
                limpo = Regex.Replace(limpo, $@"\b{ruido}\b", " ");
            }

            limpo = Regex.Replace(limpo, @"\s+", " ").Trim();
            return limpo;
        }

        public static string NormalizarEditoraCanonico(string? editora)
        {
            if (string.IsNullOrWhiteSpace(editora)) return string.Empty;
            var limpo = editora.Trim().ToLowerInvariant();

            if (EditorasCanonicas.TryGetValue(limpo, out var canonica))
            {
                return canonica;
            }

            foreach (var kvp in EditorasCanonicas)
            {
                if (limpo.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return editora.Trim();
        }

        private static double CalcularLevenshteinRatio(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 1.0 : 0.0;
            if (string.IsNullOrEmpty(t)) return 0.0;
            if (s == t) return 1.0;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            int dist = d[n, m];
            int maxLen = Math.Max(n, m);
            return 1.0 - ((double)dist / maxLen);
        }

        private static double CalcularJaroWinkler(string s1, string s2)
        {
            if (s1 == s2) return 1.0;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

            int len1 = s1.Length;
            int len2 = s2.Length;
            int matchDistance = Math.Max(len1, len2) / 2 - 1;

            bool[] s1Matches = new bool[len1];
            bool[] s2Matches = new bool[len2];

            int matches = 0;
            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, len2);

                for (int j = start; j < end; j++)
                {
                    if (s2Matches[j]) continue;
                    if (s1[i] != s2[j]) continue;
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }

            if (matches == 0) return 0.0;

            int transpositions = 0;
            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[k]) k++;
                if (s1[i] != s2[k]) transpositions++;
                k++;
            }

            double jaro = ((matches / (double)len1) + (matches / (double)len2) + ((matches - transpositions / 2.0) / matches)) / 3.0;

            // Winkler prefix bonus
            int prefix = 0;
            for (int i = 0; i < Math.Min(4, Math.Min(len1, len2)); i++)
            {
                if (s1[i] == s2[i]) prefix++;
                else break;
            }

            return jaro + (prefix * 0.1 * (1.0 - jaro));
        }

        private static double CalcularTokenSetRatio(string s1, string s2)
        {
            var tokens1 = new HashSet<string>(s1.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var tokens2 = new HashSet<string>(s2.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (tokens1.Count == 0 && tokens2.Count == 0) return 1.0;
            if (tokens1.Count == 0 || tokens2.Count == 0) return 0.0;

            var intersection = new HashSet<string>(tokens1);
            intersection.IntersectWith(tokens2);

            var union = new HashSet<string>(tokens1);
            union.UnionWith(tokens2);

            return (double)intersection.Count / union.Count;
        }

        #endregion
    }
}
