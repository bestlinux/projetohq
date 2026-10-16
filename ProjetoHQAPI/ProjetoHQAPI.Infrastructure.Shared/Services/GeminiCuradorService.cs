using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class GeminiCuradorService : ICuradorService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly IHQRepository _hqRepository;
        private readonly IDesejoRepository _desejoRepository;
        private readonly ILogger<GeminiCuradorService> _logger;

        private readonly string _apiKey;
        private readonly string _model;

        private static List<HQ>? _cachedHqs;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        private async Task<List<HQ>> ObterTodasHqsAsync(CancellationToken cancellationToken)
        {
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                if (_cachedHqs != null && _cachedHqs.Count > 0 && (DateTime.UtcNow - _lastCacheTime).TotalMinutes < 30)
                {
                    return _cachedHqs;
                }

                _logger.LogInformation("Carregando HQs do acervo para memória cache...");
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromMinutes(3));

                try
                {
                    var list = await _hqRepository.GetCuradorHQsAsync(cts.Token);
                    if (list != null && list.Count > 0)
                    {
                        _cachedHqs = list;
                        _lastCacheTime = DateTime.UtcNow;
                        _logger.LogInformation("Cache de HQs inicializado com {Count} registros.", list.Count);
                        return list;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao carregar HQs via GetCuradorHQsAsync.");
                }

                return _cachedHqs ?? new List<HQ>();
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private static void InvalidarCacheHqs()
        {
            _cachedHqs = null;
        }

        private readonly ITelemetriaIaService _telemetriaService;

        public GeminiCuradorService(
            IConfiguration configuration,
            IHQRepository hqRepository,
            IDesejoRepository desejoRepository,
            ITelemetriaIaService telemetriaService,
            ILogger<GeminiCuradorService> logger)
        {
            _configuration = configuration;
            _hqRepository = hqRepository;
            _desejoRepository = desejoRepository;
            _telemetriaService = telemetriaService;
            _logger = logger;

            _apiKey = _configuration["Gemini:ApiKey"] 
                   ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                   ?? string.Empty;

            _model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        }

        #region Chamada Central ao Gemini
        private async Task<string> CallGeminiAsync(
            string prompt, 
            bool expectJson, 
            double temperature = 0.3, 
            CancellationToken cancellationToken = default,
            string funcionalidade = "Curador_Geral", 
            int qtdFerramentas = 0)
        {
            var sw = Stopwatch.StartNew();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new JsonObject
            {
                ["contents"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject { ["text"] = prompt }
                        }
                    }
                },
                ["generationConfig"] = new JsonObject
                {
                    ["temperature"] = temperature
                }
            };

            if (expectJson)
            {
                requestBody["generationConfig"]!["responseMimeType"] = "application/json";
            }

            var jsonContent = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("Enviando requisição ao Gemini ({Model}) para {Funcionalidade}...", _model, funcionalidade);
                var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro na chamada do Gemini ({StatusCode}): {Body}", response.StatusCode, responseString);
                    _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
                    {
                        Funcionalidade = funcionalidade,
                        Operacao = "chat.completions",
                        Modelo = _model,
                        OrigemMotor = "GEMINI_SYSTEM2_CLOUD",
                        PromptTokens = prompt.Length / 4,
                        CompletionTokens = 0,
                        QtdFerramentas = qtdFerramentas,
                        LatenciaMs = sw.ElapsedMilliseconds,
                        Sucesso = false,
                        CodigoStatus = (int)response.StatusCode,
                        MensagemErro = $"Erro Gemini {response.StatusCode}"
                    }, cancellationToken);

                    throw new InvalidOperationException($"Erro na API Gemini: {response.StatusCode} - {responseString}");
                }

                var root = JsonNode.Parse(responseString);
                var text = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                int promptTokens = root?["usageMetadata"]?["promptTokenCount"]?.GetValue<int>() ?? (prompt.Length / 4);
                int completionTokens = root?["usageMetadata"]?["candidatesTokenCount"]?.GetValue<int>() ?? ((text?.Length ?? 0) / 4);

                _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
                {
                    Funcionalidade = funcionalidade,
                    Operacao = "chat.completions",
                    Modelo = _model,
                    OrigemMotor = "GEMINI_SYSTEM2_CLOUD",
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    QtdFerramentas = qtdFerramentas,
                    LatenciaMs = sw.ElapsedMilliseconds,
                    Sucesso = true,
                    CodigoStatus = (int)response.StatusCode
                }, cancellationToken);

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Gemini retornou texto nulo ou vazio. Resposta bruta: {Raw}", responseString);
                    throw new InvalidOperationException("Gemini retornou conteúdo vazio.");
                }

                _logger.LogInformation("Resposta do Gemini recebida com sucesso ({Length} chars, {Latencia}ms).", text.Length, sw.ElapsedMilliseconds);
                return text.Trim();
            }
            catch (Exception ex)
            {
                if (sw.IsRunning) sw.Stop();
                _ = _telemetriaService.RegistrarChamadaAsync(new RegistroChamadaIaInput
                {
                    Funcionalidade = funcionalidade,
                    Operacao = "chat.completions",
                    Modelo = _model,
                    OrigemMotor = "GEMINI_SYSTEM2_CLOUD",
                    PromptTokens = prompt.Length / 4,
                    CompletionTokens = 0,
                    QtdFerramentas = qtdFerramentas,
                    LatenciaMs = sw.ElapsedMilliseconds,
                    Sucesso = false,
                    CodigoStatus = 500,
                    MensagemErro = ex.Message
                }, cancellationToken);

                _logger.LogError(ex, "Falha ao consultar Gemini: {Message}", ex.Message);
                throw;
            }
        }
        #endregion

        #region 1. Guia Cronológico de Leitura & Ordem de Sagas
        public async Task<OrdemLeituraResponse> GerarOrdemLeituraAsync(OrdemLeituraRequest request, CancellationToken cancellationToken = default)
        {
            var allHqs = await ObterTodasHqsAsync(cancellationToken);

            var prompt = $@"
Você é o mais consagrado especialista em quadrinhos do mundo. 
Construa a ordem cronológica ideal e definitiva de leitura para a seguinte saga, personagem ou evento: '{request.Query}'.
Universo/Editora: '{request.Universo ?? "Geral"}'.

Considere arcos preliminares importantes, a minissérie ou evento central, tie-ins cruciais e o epílogo/consequências.
Ordene cronologicamente para a melhor experiência narrativa.

Retorne ESTRITAMENTE um JSON no seguinte formato (sem markdown em volta):
{{
  ""saga"": ""Nome canônico da Saga"",
  ""universo"": ""Marvel / DC / Vertigo / etc"",
  ""descricaoGeral"": ""Resumo conciso de 2 parágrafos sobre a premissa e importância desta saga"",
  ""itens"": [
    {{
      ""ordem"": 1,
      ""titulo"": ""Título da HQ ou Edição específica"",
      ""editoraRecomendada"": ""Panini / Abril / etc"",
      ""numeroEdicao"": 1,
      ""importancia"": ""Essencial / Prelúdio / Tie-in Principal / Epílogo"",
      ""sinopseCurta"": ""Por que ler este volume nesta posição""
    }}
  ]
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.2, cancellationToken);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<OrdemLeituraResponse>(jsonResult, options) ?? new OrdemLeituraResponse();

            // Cruzamento com o Acervo Real do Usuário
            int naEstanteCount = 0;
            foreach (var item in response.Itens)
            {
                var match = allHqs.FirstOrDefault(h => 
                    IsSimilarTitle(h.Titulo, item.Titulo) && 
                    (item.NumeroEdicao <= 0 || h.NumeroEdicao == item.NumeroEdicao || Math.Abs(h.NumeroEdicao - item.NumeroEdicao) == 0));

                if (match != null)
                {
                    item.NaEstante = true;
                    item.HqId = match.Id;
                    item.Lido = match.Lido == 1;
                    naEstanteCount++;
                }
                else
                {
                    item.NaEstante = false;
                }
            }

            response.TotalItensNaEstante = naEstanteCount;
            response.TotalItensAusentes = response.Itens.Count - naEstanteCount;

            return response;
        }
        #endregion

        #region 2. Detetive de Coleção & DNA do Acervo (com detecção algorítmica de Gaps)
        public async Task<DnaColecaoResponse> AnalisarDnaColecaoEGapsAsync(CancellationToken cancellationToken = default)
        {
            var allHqs = await ObterTodasHqsAsync(cancellationToken);

            var totalHqs = allHqs.Count;
            var totalLidas = allHqs.Count(h => h.Lido == 1);
            var porcentagemLidas = totalHqs > 0 ? Math.Round((double)totalLidas / totalHqs * 100, 1) : 0;

            // Estatísticas de Editoras
            var distEditoras = allHqs
                .GroupBy(h => string.IsNullOrWhiteSpace(h.Editora) ? "Outras" : h.Editora.Trim())
                .Select(g => new MetricaPercentualDto
                {
                    Nome = g.Key,
                    Quantidade = g.Count(),
                    Porcentagem = totalHqs > 0 ? Math.Round((double)g.Count() / totalHqs * 100, 1) : 0
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(5)
                .ToList();

            // Algoritmo Determinístico de Detecção de Lacunas (Gaps)
            var gaps = DetectarLacunasSequenciais(allHqs);

            // Resumo para o Gemini formular o Arquétipo
            var topEditorasTexto = string.Join(", ", distEditoras.Select(e => $"{e.Nome} ({e.Quantidade})"));
            var titulosAmostra = string.Join("; ", allHqs.Take(30).Select(h => $"{h.Titulo} #{h.NumeroEdicao} ({h.Editora})"));

            var prompt = $@"
Você é um analista especialista em hábitos de colecionismo de quadrinhos e arquétipos de leitores.
Analise os seguintes dados do acervo de um usuário:
- Total de HQs: {totalHqs}
- HQs Lidas: {totalLidas} ({porcentagemLidas}%)
- Principais Editoras: {topEditorasTexto}
- Amostra de títulos: {titulosAmostra}

Defina um perfil psicológico e arquétipo fascinante para este colecionador.
Gere também 3 a 5 recomendações de próximas aquisições que combinem cirurgicamente com seu DNA e preencham seus gostos.

Retorne ESTRITAMENTE um JSON:
{{
  ""arquetipo"": ""Nome criativo (ex: O Arquiteto do Multiverso, O Completista Sombrio, Guardião da Era de Bronze, etc)"",
  ""subtituloArquetipo"": ""Frase de impacto definindo a essência"",
  ""descricaoArquetipo"": ""Análise detalhada de 2 parágrafos sobre a identidade de colecionador revelada pelos dados"",
  ""pontosFortesColecao"": [
    ""Ponto forte 1"",
    ""Ponto forte 2"",
    ""Ponto forte 3""
  ],
  ""recomendacoesProximasAquisicoes"": [
    {{
      ""titulo"": ""Nome da HQ sugerida"",
      ""editora"": ""Editora recomendada"",
      ""numeroEdicao"": 1,
      ""motivo"": ""Por que esta obra é o próximo passo perfeito para o acervo"",
      ""matchScore"": ""98%""
    }}
  ]
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.3, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<DnaColecaoResponse>(jsonResult, options) ?? new DnaColecaoResponse();

            response.TotalHqs = totalHqs;
            response.TotalLidas = totalLidas;
            response.PorcentagemLidas = porcentagemLidas;
            response.DistribuicaoEditoras = distEditoras;
            response.GapsDetectados = gaps;

            return response;
        }

        private List<GapSerieDto> DetectarLacunasSequenciais(List<HQ> allHqs)
        {
            var gaps = new List<GapSerieDto>();

            // Agrupa por série simplificada (remove edições ou volumes no final)
            var seriesGroup = allHqs
                .Where(h => h.NumeroEdicao > 0)
                .GroupBy(h => new
                {
                    Serie = NormalizarNomeSerie(h.Titulo ?? ""),
                    Editora = string.IsNullOrWhiteSpace(h.Editora) ? "Desconhecida" : h.Editora.Trim()
                })
                .Where(g => g.Count() >= 2); // precisa ter pelo menos 2 edições para identificar sequência

            foreach (var group in seriesGroup)
            {
                var edicoesPossuidas = group.Select(x => x.NumeroEdicao).Distinct().OrderBy(x => x).ToList();
                if (edicoesPossuidas.Count < 2) continue;

                var min = edicoesPossuidas.First();
                var max = edicoesPossuidas.Last();

                // Procura números faltantes no intervalo
                var faltantes = new List<int>();
                for (int i = min; i <= max; i++)
                {
                    if (!edicoesPossuidas.Contains(i))
                    {
                        faltantes.Add(i);
                    }
                }

                if (faltantes.Count > 0 && faltantes.Count <= 8)
                {
                    var urgencia = faltantes.Count == 1 ? "Alta" : "Média";
                    var motivo = faltantes.Count == 1 
                        ? $"Falta apenas a edição #{faltantes[0]} para fechar a sequência de #{min} a #{max}!"
                        : $"Faltam as edições [{string.Join(", ", faltantes)}] para completar o arco.";

                    gaps.Add(new GapSerieDto
                    {
                        Serie = group.Key.Serie,
                        Editora = group.Key.Editora,
                        EdicoesPossuidas = edicoesPossuidas,
                        EdicoesFaltantes = faltantes,
                        Urgencia = urgencia,
                        MotivoGap = motivo
                    });
                }
            }

            return gaps.OrderByDescending(g => g.Urgencia == "Alta").Take(10).ToList();
        }

        private string NormalizarNomeSerie(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return "Diversos";
            var clean = Regex.Replace(titulo, @"(?i)(:\s*.*|\s+-\s+.*|\s+vol(ume)?\.?\s*\d+.*|\s+n[º°\.]?\s*\d+.*|#\d+)", "").Trim();
            return clean.Length > 2 ? clean : titulo.Trim();
        }
        #endregion

        #region 3. Storyteller / Aquecimento Narrativo Imersivo
        public async Task<RecapNarrativoResponse> GerarRecapNarrativoAsync(RecapNarrativoRequest request, CancellationToken cancellationToken = default)
        {
            var prompt = $@"
Você é um narrador dramático de histórias em quadrinhos no formato consagrado 'PREVIOUSLY ON...'.
Crie um texto de aquecimento narrativo imersivo, atmosférico e empolgante para um leitor que está prestes a abrir a HQ:
- Título: '{request.Titulo}'
- Edição: #{request.NumeroEdicao ?? 1}
- Editora: '{request.Editora ?? "Independente"}'

O objetivo é situar o leitor:
- O que aconteceu imediatamente antes dessa história ou edição?
- Qual é o grande arco narrativo em que ela se insere?
- Quais personagens estão no centro do conflito e suas motivações?
- Qual o tom/clima ideal para desfrutar da leitura?

Retorne ESTRITAMENTE um JSON:
{{
  ""titulo"": ""{request.Titulo}"",
  ""numeroEdicao"": {request.NumeroEdicao ?? 1},
  ""arcoNarrativo"": ""Nome do arco narrativo ou fase"",
  ""previouslyOnText"": ""Texto dramático e envolvente de 3 parágrafos narrando os antecedentes com paixão"",
  ""personagensChave"": [""Personagem 1"", ""Personagem 2"", ""Personagem 3""],
  ""eventosPrecedentes"": [
    ""Evento anterior 1"",
    ""Evento anterior 2"",
    ""Evento anterior 3""
  ],
  ""climaOuTom"": ""Sombrio, Investigativo e Psicológico / Ação Frenética / Épico Cósmico / etc""
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.4, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<RecapNarrativoResponse>(jsonResult, options) ?? new RecapNarrativoResponse();
        }
        #endregion

        #region 4. Trivia & Quiz Interativo do Acervo
        public async Task<QuizAcervoResponse> GerarQuizAcervoAsync(int quantidadePerguntas = 5, CancellationToken cancellationToken = default)
        {
            var allHqs = await ObterTodasHqsAsync(cancellationToken);
            string listaHqsTexto;

            if (allHqs.Count > 0)
            {
                var rnd = new Random();
                var amostraHqs = allHqs.OrderBy(_ => rnd.Next()).Take(15).ToList();
                listaHqsTexto = string.Join("\n", amostraHqs.Select(h => $"- {h.Titulo} #{h.NumeroEdicao} (Editora: {h.Editora}, Personagens: {h.Personagens})"));
            }
            else
            {
                listaHqsTexto = @"- Batman: Ano Um (DC Comics, Frank Miller)
- Watchmen (DC / Vertigo, Alan Moore)
- Homem-Aranha: A Última Caçada de Kraven (Marvel)
- Sandman: Prelúdios e Noturnos (Vertigo, Neil Gaiman)
- Berserk (Panini, Kentaro Miura)
- X-Men: Dias de um Futuro Esquecido (Marvel, Chris Claremont)";
            }

            var prompt = $@"
Você é o mestre dos quizzes de quadrinhos. Crie um Quiz dinâmico e divertido de {quantidadePerguntas} perguntas de múltipla escolha BASEADAS ESTRITAMENTE nas HQs e personagens presentes no acervo do usuário:
{listaHqsTexto}

Cada pergunta deve:
- Ter 4 opções distintas (A, B, C, D)
- Ter apenas 1 resposta correta indicada pelo índice numérico (0, 1, 2 ou 3)
- Conter uma explicação rica e curiosa sobre a resposta
- Indicar em qual HQ da coleção a pergunta foi inspirada

Retorne ESTRITAMENTE um JSON:
{{
  ""perguntas"": [
    {{
      ""id"": 1,
      ""pergunta"": ""Pergunta instigante sobre trama, personagens ou curiosidade histórica da HQ?"",
      ""opcoes"": [
        ""Opção A"",
        ""Opção B"",
        ""Opção C"",
        ""Opção D""
      ],
      ""respostaCorretaIndex"": 0,
      ""explicacao"": ""Explicação fascinante sobre o porquê desta resposta"",
      ""hqReferencia"": ""Nome da HQ do usuário""
    }}
  ]
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.4, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<QuizAcervoResponse>(jsonResult, options) ?? new QuizAcervoResponse();
        }
        #endregion

        #region 5. Curador Virtual com RAG em Memória
        public async Task<ChatbotResponse> ConsultarChatbotColecaoAsync(ChatbotRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Estatísticas ultra-rápidas via SQL direto (<10ms)
            var stats = await _hqRepository.ObterEstatisticasCuradorAsync(cancellationToken);
            
            // 2. Busca cirúrgica por termos diretamente no banco (<20ms)
            var hqsRelevantes = await _hqRepository.BuscarHqsParaCuradorAsync(request.Mensagem, cancellationToken);

            var catalogoResumido = new StringBuilder();
            catalogoResumido.AppendLine($"ESTATÍSTICAS GERAIS DA COLEÇÃO DO USUÁRIO:");
            catalogoResumido.AppendLine($"- Total de HQs cadastradas no acervo: {stats.total}");
            catalogoResumido.AppendLine($"- Total de HQs lidas: {stats.lidos}");
            catalogoResumido.AppendLine($"- Total de HQs não lidas na fila de leitura: {stats.naoLidos}");

            if (hqsRelevantes.Count > 0)
            {
                catalogoResumido.AppendLine();
                catalogoResumido.AppendLine($"HQS DO ACERVO DO USUÁRIO IDENTIFICADAS COMO DIRETAMENTE RELEVANTES PARA A PERGUNTA:");
                foreach (var hq in hqsRelevantes)
                {
                    var lido = hq.Lido == 1 ? "Lido" : "Não lido";
                    var autor = string.IsNullOrWhiteSpace(hq.DesenhosRoteirosArteFinalCores) ? "" : $" | Roteiro/Arte: {hq.DesenhosRoteirosArteFinalCores}";
                    catalogoResumido.AppendLine($"- {hq.Titulo} #{hq.NumeroEdicao} ({hq.Editora}, {hq.DataPublicacao}) | Status: {lido}{autor}");
                }
            }
            else
            {
                var amostra = await _hqRepository.ObterAmostraEstanteCuradorAsync(50, cancellationToken);
                catalogoResumido.AppendLine();
                catalogoResumido.AppendLine("AMOSTRA REPRESENTATIVA DE HQS NA ESTANTE DO USUÁRIO:");
                foreach (var hq in amostra)
                {
                    var lido = hq.Lido == 1 ? "Lido" : "Não lido";
                    catalogoResumido.AppendLine($"- {hq.Titulo} #{hq.NumeroEdicao} ({hq.Editora}) | Status: {lido}");
                }
            }

            var historicoTexto = string.Join("\n", request.Historico.TakeLast(6).Select(m => $"{m.Remetente}: {m.Texto}"));

            var prompt = $@"
Você é o 'Curador Virtual' do ProjetoHQ, um consultor de inteligência artificial sábio, bem-humorado e apaixonado pela nona arte.
Você possui em sua memória imediata os dados reais do acervo do usuário:
[CONTEXTO DO ACERVO]
{catalogoResumido}
[FIM DO CONTEXTO]

Histórico recente da conversa:
{historicoTexto}

Mensagem atual do usuário: ""{request.Mensagem}""

Instruções:
- Se o usuário perguntar o que ele possui ou tem de determinado autor, personagem ou editora (ex: 'O que eu tenho do Alan Moore?'):
  * Verifique OBRIGATORIAMENTE a seção 'HQS DO ACERVO DO USUÁRIO IDENTIFICADAS COMO DIRETAMENTE RELEVANTES PARA A PERGUNTA' acima.
  * Se houver obras listadas nessa seção, o usuário EFETIVAMENTE POSSUI essas HQs! Liste-as com detalhes de título, número da edição, editora e se já foram lidas ou não.
  * JAMAIS diga que a estante está zerada se houver HQs cadastradas e listadas no contexto.
- Se o usuário pedir recomendações com base em humor, clima ou gênero, recomende primeiro obras que ele já tem na estante que ainda não foram lidas, ou clássicos indispensáveis.
- Indique os títulos exatos das HQs que você mencionou na chave 'hqsMencionadas'.
- Forneça 3 sugestões de perguntas rápidas de continuação que o usuário possa clicar em seguida.

Retorne ESTRITAMENTE um JSON:
{{
  ""resposta"": ""Sua resposta rica formatada em markdown amigável"",
  ""hqsMencionadas"": [""HQ 1"", ""HQ 2""],
  ""sugestoesPerguntas"": [
    ""Pergunta sugestão 1?"",
    ""Pergunta sugestão 2?"",
    ""Pergunta sugestão 3?""
  ]
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.3, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ChatbotResponse>(jsonResult, options) ?? new ChatbotResponse();
        }
        #endregion

        #region 6. Comandos CRUD em Linguagem Natural
        public async Task<ComandoCrudPreviewResponse> ProcessarComandoCrudAssistidoAsync(string comando, CancellationToken cancellationToken = default)
        {
            var allHqs = await ObterTodasHqsAsync(cancellationToken);

            var amostraHqs = string.Join("\n", allHqs.Take(40).Select(h => $"Id: {h.Id} | {h.Titulo} #{h.NumeroEdicao} ({h.Editora}) | Lido: {h.Lido}"));

            var prompt = $@"
Você é um assistente de banco de dados para coleções de HQs.
O usuário digitou o seguinte comando em linguagem natural:
""{comando}""

Amostra de HQs já cadastradas na coleção:
{amostraHqs}

Analise a intenção do usuário e extraia a ação:
Ações possíveis:
- ""marcar_lido"" (se o usuário disse 'li Batman', 'marque X como lido', etc)
- ""desmarcar_lido"" (se disse 'desmarque X como lido', 'não li ainda')
- ""criar_hq"" (se disse 'adicione X na estante/coleção', 'comprei X')
- ""criar_desejo"" (se disse 'coloque X na lista de desejos / quero comprar')
- ""excluir_hq"" (se disse 'remova X da coleção', 'exclua X')

Se referenciar uma HQ existente na coleção, encontre o Id exato da amostra ou sugira o título correspondente.

Retorne ESTRITAMENTE um JSON:
{{
  ""comandoOriginal"": ""{comando}"",
  ""acao"": ""marcar_lido / desmarcar_lido / criar_hq / criar_desejo / excluir_hq"",
  ""interpretacaoHumana"": ""Explicação clara em português do que será executado (ex: Marcar 'Batman: Ano Um' #1 como lido)"",
  ""entidadeAlvo"": ""HQ"",
  ""hqIdAlvo"": ""Guid do registro caso seja uma HQ existente ou null"",
  ""tituloAlvo"": ""Título extraído"",
  ""editoraAlvo"": ""Editora extraída ou Panini se não informado"",
  ""edicaoAlvo"": 1,
  ""parametrosModificadosJson"": ""Resumo dos campos afetados"",
  ""requerConfirmacao"": true
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.1, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var preview = JsonSerializer.Deserialize<ComandoCrudPreviewResponse>(jsonResult, options) ?? new ComandoCrudPreviewResponse();

            // Valida se o ID existe de fato na base ou faz match por aproximação
            if (!preview.HqIdAlvo.HasValue && !string.IsNullOrWhiteSpace(preview.TituloAlvo))
            {
                var match = allHqs.FirstOrDefault(h => IsSimilarTitle(h.Titulo, preview.TituloAlvo));
                if (match != null)
                {
                    preview.HqIdAlvo = match.Id;
                    preview.TituloAlvo = match.Titulo;
                    preview.EditoraAlvo = match.Editora;
                    preview.EdicaoAlvo = match.NumeroEdicao;
                }
            }

            return preview;
        }

        public async Task<ComandoCrudExecutarResult> ExecutarComandoCrudAsync(ComandoCrudExecutarRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                switch (request.Acao?.ToLowerInvariant())
                {
                    case "marcar_lido":
                    case "desmarcar_lido":
                        if (!request.HqIdAlvo.HasValue)
                        {
                            return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = "ID da HQ alvo não fornecido." };
                        }
                        var hq = await _hqRepository.GetByIdAsync(request.HqIdAlvo.Value);
                        if (hq == null)
                        {
                            return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = "HQ não encontrada na coleção." };
                        }
                        hq.Lido = request.Acao.ToLowerInvariant() == "marcar_lido" ? 1 : 0;
                        await _hqRepository.UpdateAsync(hq);
                        return new ComandoCrudExecutarResult
                        {
                            Sucesso = true,
                            Mensagem = $"HQ '{hq.Titulo}' marcada como {(hq.Lido == 1 ? "lida" : "não lida")} com sucesso!",
                            IdRegistroAfetado = hq.Id
                        };

                    case "criar_hq":
                        var novaHq = new HQ
                        {
                            Titulo = request.Titulo?.Trim() ?? "Nova HQ",
                            Editora = request.Editora?.Trim() ?? "Independente",
                            NumeroEdicao = request.NumeroEdicao > 0 ? request.NumeroEdicao : 1,
                            Preco = request.Preco ?? "R$ 0,00",
                            Lido = request.Lido ?? 0
                        };
                        await _hqRepository.AddAsync(novaHq);
                        InvalidarCacheHqs();
                        return new ComandoCrudExecutarResult
                        {
                            Sucesso = true,
                            Mensagem = $"HQ '{novaHq.Titulo} #{novaHq.NumeroEdicao}' adicionada à coleção com sucesso!",
                            IdRegistroAfetado = novaHq.Id
                        };

                    case "criar_desejo":
                        var novoDesejo = new Desejo
                        {
                            Titulo = request.Titulo?.Trim() ?? "HQ Desejada",
                            Editora = request.Editora?.Trim() ?? "Independente",
                            NumeroEdicao = request.NumeroEdicao > 0 ? request.NumeroEdicao : 1,
                            Preco = request.Preco ?? "R$ 0,00"
                        };
                        await _desejoRepository.AddAsync(novoDesejo);
                        return new ComandoCrudExecutarResult
                        {
                            Sucesso = true,
                            Mensagem = $"'{novoDesejo.Titulo} #{novoDesejo.NumeroEdicao}' adicionado à Lista de Desejos!",
                            IdRegistroAfetado = novoDesejo.Id
                        };

                    case "excluir_hq":
                        if (!request.HqIdAlvo.HasValue)
                        {
                            return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = "ID da HQ alvo não fornecido para exclusão." };
                        }
                        var hqExcluir = await _hqRepository.GetByIdAsync(request.HqIdAlvo.Value);
                        if (hqExcluir == null)
                        {
                            return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = "HQ não encontrada." };
                        }
                        await _hqRepository.RemoveAsync(request.HqIdAlvo.Value);
                        InvalidarCacheHqs();
                        return new ComandoCrudExecutarResult
                        {
                            Sucesso = true,
                            Mensagem = $"HQ '{hqExcluir.Titulo}' excluída da coleção.",
                            IdRegistroAfetado = hqExcluir.Id
                        };

                    default:
                        return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = $"Ação desconhecida: {request.Acao}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando CRUD: {Message}", ex.Message);
                return new ComandoCrudExecutarResult { Sucesso = false, Mensagem = $"Erro ao executar: {ex.Message}" };
            }
        }
        #endregion

        #region 7. Importação em Lote Assistida
        public async Task<ImportacaoLotePreviewResponse> GerarImportacaoLoteAsync(string textoLote, CancellationToken cancellationToken = default)
        {
            var allHqs = await ObterTodasHqsAsync(cancellationToken);

            var prompt = $@"
Você é um motor de ingestão catalográfica de quadrinhos.
O usuário colou a seguinte lista desestruturada ou intervalo de HQs:
---
{textoLote}
---

Instruções:
- Se houver intervalos numéricos (ex: 'volumes 1 ao 10 de Berserk da Panini', 'Spawn 1 a 5 Abril'), desdobre em cada edição individual (1, 2, 3, etc).
- Para cada item, extraia: Título principal da série, Editora inferida, Número da Edição, Ano aproximado, Categoria (1 = Revista, 2 = Álbum/Especial), Gênero (1 = Super-heróis, 2 = Mangá, 3 = Terror, 4 = Ficção, etc).
- Máximo de 50 edições geradas por chamada para segurança.

Retorne ESTRITAMENTE um JSON:
{{
  ""totalDetectados"": 5,
  ""itens"": [
    {{
      ""idTemp"": 1,
      ""titulo"": ""Nome limpo da série"",
      ""editora"": ""Panini / JBC / Abril / etc"",
      ""numeroEdicao"": 1,
      ""anoLancamento"": ""2022"",
      ""categoria"": 1,
      ""genero"": 1,
      ""precoSugerido"": ""R$ 39,90""
    }}
  ]
}}";

            var jsonResult = await CallGeminiAsync(prompt, expectJson: true, temperature: 0.1, cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<ImportacaoLotePreviewResponse>(jsonResult, options) ?? new ImportacaoLotePreviewResponse();

            int idCounter = 1;
            foreach (var item in response.Itens)
            {
                item.IdTemp = idCounter++;
                item.JaExisteNaColecao = allHqs.Any(h => 
                    IsSimilarTitle(h.Titulo, item.Titulo) && 
                    h.NumeroEdicao == item.NumeroEdicao);
                item.Selecionado = !item.JaExisteNaColecao;
            }

            response.TotalDetectados = response.Itens.Count;
            return response;
        }

        public async Task<ImportacaoLoteSalvarResult> SalvarImportacaoLoteAsync(ImportacaoLoteSalvarRequest request, CancellationToken cancellationToken = default)
        {
            int salvos = 0;
            var itensParaSalvar = request.Itens.Where(i => i.Selecionado).ToList();

            foreach (var item in itensParaSalvar)
            {
                if (request.Destino?.ToLowerInvariant() == "desejos")
                {
                    var desejo = new Desejo
                    {
                        Titulo = item.Titulo.Trim(),
                        Editora = item.Editora.Trim(),
                        NumeroEdicao = item.NumeroEdicao,
                        Categoria = item.Categoria,
                        Genero = item.Genero,
                        DataPublicacao = item.AnoLancamento,
                        Preco = item.PrecoSugerido
                    };
                    await _desejoRepository.AddAsync(desejo);
                }
                else
                {
                    var hq = new HQ
                    {
                        Titulo = item.Titulo.Trim(),
                        Editora = item.Editora.Trim(),
                        NumeroEdicao = item.NumeroEdicao,
                        Categoria = item.Categoria,
                        Genero = item.Genero,
                        DataPublicacao = item.AnoLancamento,
                        Preco = item.PrecoSugerido,
                        Lido = 0
                    };
                    await _hqRepository.AddAsync(hq);
                }
                salvos++;
            }

            if (request.Destino?.ToLowerInvariant() != "desejos")
            {
                InvalidarCacheHqs();
            }

            return new ImportacaoLoteSalvarResult
            {
                TotalSalvos = salvos,
                Mensagem = $"{salvos} quadrinho(s) salvos com sucesso no destino: {request.Destino}!"
            };
        }
        #endregion

        #region Utilitários de Comparação de Título
        private static bool IsSimilarTitle(string? title1, string? title2)
        {
            if (string.IsNullOrWhiteSpace(title1) || string.IsNullOrWhiteSpace(title2)) return false;

            var t1 = Clean(title1);
            var t2 = Clean(title2);

            return t1.Equals(t2, StringComparison.OrdinalIgnoreCase) ||
                   t1.Contains(t2, StringComparison.OrdinalIgnoreCase) ||
                   t2.Contains(t1, StringComparison.OrdinalIgnoreCase);
        }

        private static string Clean(string str)
        {
            var noAccents = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(str));
            var alpha = Regex.Replace(noAccents, @"[^a-zA-Z0-9\s]", "");
            return alpha.Trim().ToLowerInvariant();
        }
        #endregion
    }
}
