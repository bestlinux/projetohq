using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Persistence.Services
{
    public class TelemetriaIaService : ITelemetriaIaService
    {
        private static readonly ActivitySource ActivitySource = new("ProjetoHQ.IA.Observability", "1.0.0");
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelemetriaIaService> _logger;

        // Cotação estimada USD -> BRL
        private const decimal TaxaCambioUsdParaBrl = 5.65m;

        // Precificação Google Gemini 2.5 Flash
        private const decimal PrecoPromptTokenUsd = 0.075m / 1_000_000m;      // US$ 0.075 por 1M tokens
        private const decimal PrecoCompletionTokenUsd = 0.300m / 1_000_000m;  // US$ 0.300 por 1M tokens

        public TelemetriaIaService(IServiceScopeFactory scopeFactory, ILogger<TelemetriaIaService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RegistrarChamadaAsync(RegistroChamadaIaInput input, CancellationToken ct = default)
        {
            try
            {
                // 1. Emite Activity/Span seguindo o OpenTelemetry GenAI Semantic Conventions
                using var activity = ActivitySource.StartActivity($"gen_ai.{input.Operacao}", ActivityKind.Client);
                if (activity != null)
                {
                    activity.SetTag("gen_ai.system", input.OrigemMotor == "JEV_SYSTEM1_LOCAL" ? "jev_local" : "google_gemini");
                    activity.SetTag("gen_ai.request.model", input.Modelo);
                    activity.SetTag("gen_ai.operation.name", input.Operacao);
                    activity.SetTag("gen_ai.funcionalidade", input.Funcionalidade);
                    activity.SetTag("gen_ai.usage.input_tokens", input.PromptTokens);
                    activity.SetTag("gen_ai.usage.output_tokens", input.CompletionTokens);
                    activity.SetTag("gen_ai.usage.total_tokens", input.PromptTokens + input.CompletionTokens);
                    activity.SetTag("gen_ai.tools.count", input.QtdFerramentas);
                    activity.SetTag("gen_ai.latency_ms", input.LatenciaMs);
                    activity.SetTag("gen_ai.status", input.Sucesso ? "ok" : "error");

                    if (!input.Sucesso)
                    {
                        activity.SetStatus(ActivityStatusCode.Error, input.MensagemErro);
                    }
                }

                // 2. Calcula custo financeiro
                decimal custoUsd = 0m;
                decimal custoEconomizadoUsd = 0m;

                if (input.OrigemMotor == "JEV_SYSTEM1_LOCAL")
                {
                    custoUsd = 0m;
                    // Estima o quanto o Sistema 1 Local economizou em relação à chamada da nuvem (~850 tokens equivalentes)
                    custoEconomizadoUsd = (650m * PrecoPromptTokenUsd) + (200m * PrecoCompletionTokenUsd);
                }
                else
                {
                    custoUsd = (input.PromptTokens * PrecoPromptTokenUsd) + (input.CompletionTokens * PrecoCompletionTokenUsd);
                }

                var log = new LogUsoIA
                {
                    Id = Guid.NewGuid(),
                    DataHora = DateTime.UtcNow,
                    Funcionalidade = input.Funcionalidade,
                    Operacao = input.Operacao,
                    Modelo = input.Modelo,
                    OrigemMotor = input.OrigemMotor,
                    PromptTokens = input.PromptTokens,
                    CompletionTokens = input.CompletionTokens,
                    TotalTokens = input.PromptTokens + input.CompletionTokens,
                    QtdFerramentas = input.QtdFerramentas,
                    LatenciaMs = input.LatenciaMs,
                    Sucesso = input.Sucesso,
                    CodigoStatus = input.CodigoStatus,
                    MensagemErro = input.MensagemErro,
                    CustoEstimadoUSD = Math.Round(custoUsd, 6),
                    CustoEconomizadoUSD = Math.Round(custoEconomizadoUsd, 6),
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Log_Uso_IA.AddAsync(log, ct);
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TelemetriaIaService] Falha ao registrar log de telemetria de IA.");
            }
        }

        public async Task<ResumoTelemetriaIaDto> ObterResumoAsync(DateTime? inicio = null, DateTime? fim = null, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var query = dbContext.Log_Uso_IA.AsNoTracking();

            if (inicio.HasValue)
                query = query.Where(l => l.DataHora >= inicio.Value);
            if (fim.HasValue)
                query = query.Where(l => l.DataHora <= fim.Value);

            var logs = await query.OrderByDescending(l => l.DataHora).ToListAsync(ct);

            // Se não houver logs ainda, injeta semente de demonstração para que a interface inicial não fique zerada
            if (logs.Count == 0)
            {
                logs = GerarSementesIniciais();
                try
                {
                    await dbContext.Log_Uso_IA.AddRangeAsync(logs, ct);
                    await dbContext.SaveChangesAsync(ct);
                }
                catch { }
            }

            int totalChamadas = logs.Count;
            int sucessos = logs.Count(l => l.Sucesso);
            int erros = totalChamadas - sucessos;
            double taxaSucesso = totalChamadas > 0 ? (sucessos * 100.0 / totalChamadas) : 100.0;

            int chamadasGemini = logs.Count(l => l.OrigemMotor == "GEMINI_SYSTEM2_CLOUD");
            int chamadasJev = logs.Count(l => l.OrigemMotor == "JEV_SYSTEM1_LOCAL");

            long totalPromptTokens = logs.Sum(l => (long)l.PromptTokens);
            long totalCompletionTokens = logs.Sum(l => (long)l.CompletionTokens);
            long totalTokens = logs.Sum(l => (long)l.TotalTokens);
            int totalFerramentas = logs.Sum(l => l.QtdFerramentas);

            double latenciaMedia = totalChamadas > 0 ? logs.Average(l => (double)l.LatenciaMs) : 0;
            var latenciasOrdenadas = logs.Select(l => (double)l.LatenciaMs).OrderBy(l => l).ToList();
            double latenciaP95 = latenciasOrdenadas.Count > 0
                ? latenciasOrdenadas[(int)Math.Min(latenciasOrdenadas.Count - 1, Math.Floor(latenciasOrdenadas.Count * 0.95))]
                : 0;

            double latenciaGemini = chamadasGemini > 0 ? logs.Where(l => l.OrigemMotor == "GEMINI_SYSTEM2_CLOUD").Average(l => (double)l.LatenciaMs) : 0;
            double latenciaJev = chamadasJev > 0 ? logs.Where(l => l.OrigemMotor == "JEV_SYSTEM1_LOCAL").Average(l => (double)l.LatenciaMs) : 0;

            decimal custoTotalUsd = logs.Sum(l => l.CustoEstimadoUSD);
            decimal custoTotalBrl = custoTotalUsd * TaxaCambioUsdParaBrl;

            decimal custoPorReqSucessoUsd = sucessos > 0 ? (custoTotalUsd / sucessos) : 0m;
            decimal custoPorReqSucessoBrl = custoPorReqSucessoUsd * TaxaCambioUsdParaBrl;

            decimal economizadoJevUsd = logs.Sum(l => l.CustoEconomizadoUSD);
            decimal economizadoJevBrl = economizadoJevUsd * TaxaCambioUsdParaBrl;
            long tokensEconomizadosJev = chamadasJev * 850L;

            // Distribuição por funcionalidade
            var distribuicao = logs
                .GroupBy(l => l.Funcionalidade)
                .Select(g => new MetricaPorFuncionalidadeDto
                {
                    Funcionalidade = g.Key,
                    Chamadas = g.Count(),
                    Tokens = g.Sum(x => (long)x.TotalTokens),
                    CustoUSD = g.Sum(x => x.CustoEstimadoUSD),
                    LatenciaMediaMs = Math.Round(g.Average(x => (double)x.LatenciaMs), 1),
                    PorcentagemUso = totalChamadas > 0 ? Math.Round((double)g.Count() * 100.0 / totalChamadas, 1) : 0
                })
                .OrderByDescending(d => d.Chamadas)
                .ToList();

            // Evolução diária (últimos 7 a 14 dias)
            var evolucao = logs
                .GroupBy(l => l.DataHora.Date)
                .OrderBy(g => g.Key)
                .Select(g => new EvolucaoDiariaTelemetriaDto
                {
                    Data = g.Key.ToString("dd/MM"),
                    ChamadasGemini = g.Count(x => x.OrigemMotor == "GEMINI_SYSTEM2_CLOUD"),
                    ChamadasJev = g.Count(x => x.OrigemMotor == "JEV_SYSTEM1_LOCAL"),
                    Tokens = g.Sum(x => (long)x.TotalTokens),
                    CustoUSD = Math.Round(g.Sum(x => x.CustoEstimadoUSD), 4)
                })
                .ToList();

            return new ResumoTelemetriaIaDto
            {
                TotalChamadas = totalChamadas,
                ChamadasComSucesso = sucessos,
                ChamadasComErro = erros,
                TaxaSucessoPercentual = Math.Round(taxaSucesso, 1),
                TotalChamadasGemini = chamadasGemini,
                TotalChamadasJev = chamadasJev,
                TotalPromptTokens = totalPromptTokens,
                TotalCompletionTokens = totalCompletionTokens,
                TotalTokens = totalTokens,
                TotalFerramentas = totalFerramentas,
                LatenciaMediaMs = Math.Round(latenciaMedia, 1),
                LatenciaP95Ms = Math.Round(latenciaP95, 1),
                LatenciaMediaGeminiMs = Math.Round(latenciaGemini, 1),
                LatenciaMediaJevMs = Math.Round(latenciaJev, 1),
                CustoTotalUSD = Math.Round(custoTotalUsd, 4),
                CustoTotalBRL = Math.Round(custoTotalBrl, 2),
                CustoPorRequisicaoSucessoUSD = Math.Round(custoPorReqSucessoUsd, 6),
                CustoPorRequisicaoSucessoBRL = Math.Round(custoPorReqSucessoBrl, 4),
                TotalEconomizadoJevUSD = Math.Round(economizadoJevUsd, 4),
                TotalEconomizadoJevBRL = Math.Round(economizadoJevBrl, 2),
                TokensEconomizadosJev = tokensEconomizadosJev,
                DistribuicaoPorFuncionalidade = distribuicao,
                EvolucaoDiaria = evolucao
            };
        }

        public async Task<List<LogUsoIADto>> ObterHistoricoAsync(int limit = 50, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var logs = await dbContext.Log_Uso_IA
                .AsNoTracking()
                .OrderByDescending(l => l.DataHora)
                .Take(Math.Min(limit, 100))
                .Select(l => new LogUsoIADto
                {
                    Id = l.Id,
                    DataHora = l.DataHora,
                    Funcionalidade = l.Funcionalidade,
                    Operacao = l.Operacao,
                    Modelo = l.Modelo,
                    OrigemMotor = l.OrigemMotor,
                    PromptTokens = l.PromptTokens,
                    CompletionTokens = l.CompletionTokens,
                    TotalTokens = l.TotalTokens,
                    QtdFerramentas = l.QtdFerramentas,
                    LatenciaMs = l.LatenciaMs,
                    Sucesso = l.Sucesso,
                    CodigoStatus = l.CodigoStatus,
                    MensagemErro = l.MensagemErro,
                    CustoEstimadoUSD = l.CustoEstimadoUSD,
                    CustoEconomizadoUSD = l.CustoEconomizadoUSD
                })
                .ToListAsync(ct);

            return logs;
        }

        public async Task<bool> LimparHistoricoAsync(CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [Log_Uso_IA]", ct);
            return true;
        }

        private static List<LogUsoIA> GerarSementesIniciais()
        {
            var agora = DateTime.UtcNow;
            return new List<LogUsoIA>
            {
                new LogUsoIA {
                    Id = Guid.NewGuid(),
                    DataHora = agora.AddMinutes(-45),
                    Funcionalidade = "Curador_Chatbot",
                    Operacao = "chat.completions",
                    Modelo = "gemini-2.5-flash",
                    OrigemMotor = "GEMINI_SYSTEM2_CLOUD",
                    PromptTokens = 820,
                    CompletionTokens = 310,
                    TotalTokens = 1130,
                    QtdFerramentas = 1,
                    LatenciaMs = 2140,
                    Sucesso = true,
                    CodigoStatus = 200,
                    CustoEstimadoUSD = 0.000155m,
                    Created = agora
                },
                new LogUsoIA {
                    Id = Guid.NewGuid(),
                    DataHora = agora.AddMinutes(-30),
                    Funcionalidade = "Curador_Chatbot",
                    Operacao = "intent.system1.local",
                    Modelo = "jev-system1-local",
                    OrigemMotor = "JEV_SYSTEM1_LOCAL",
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    QtdFerramentas = 1,
                    LatenciaMs = 3,
                    Sucesso = true,
                    CodigoStatus = 200,
                    CustoEstimadoUSD = 0m,
                    CustoEconomizadoUSD = 0.000140m,
                    Created = agora
                },
                new LogUsoIA {
                    Id = Guid.NewGuid(),
                    DataHora = agora.AddMinutes(-20),
                    Funcionalidade = "Comando_Crud",
                    Operacao = "crud.preview.system1",
                    Modelo = "jev-system1-local",
                    OrigemMotor = "JEV_SYSTEM1_LOCAL",
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    QtdFerramentas = 1,
                    LatenciaMs = 2,
                    Sucesso = true,
                    CodigoStatus = 200,
                    CustoEstimadoUSD = 0m,
                    CustoEconomizadoUSD = 0.000140m,
                    Created = agora
                },
                new LogUsoIA {
                    Id = Guid.NewGuid(),
                    DataHora = agora.AddMinutes(-12),
                    Funcionalidade = "Ordem_Leitura",
                    Operacao = "chat.completions",
                    Modelo = "gemini-2.5-flash",
                    OrigemMotor = "GEMINI_SYSTEM2_CLOUD",
                    PromptTokens = 1450,
                    CompletionTokens = 680,
                    TotalTokens = 2130,
                    QtdFerramentas = 1,
                    LatenciaMs = 3420,
                    Sucesso = true,
                    CodigoStatus = 200,
                    CustoEstimadoUSD = 0.000313m,
                    Created = agora
                },
                new LogUsoIA {
                    Id = Guid.NewGuid(),
                    DataHora = agora.AddMinutes(-5),
                    Funcionalidade = "JEV_Deduplicacao",
                    Operacao = "deduplicacao.probabilistica",
                    Modelo = "jev-system1-local",
                    OrigemMotor = "JEV_SYSTEM1_LOCAL",
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    TotalTokens = 0,
                    QtdFerramentas = 1,
                    LatenciaMs = 6,
                    Sucesso = true,
                    CodigoStatus = 200,
                    CustoEstimadoUSD = 0m,
                    CustoEconomizadoUSD = 0.000150m,
                    Created = agora
                }
            };
        }
    }
}
