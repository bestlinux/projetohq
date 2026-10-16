using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface ITelemetriaIaService
    {
        Task RegistrarChamadaAsync(RegistroChamadaIaInput input, CancellationToken ct = default);
        Task<ResumoTelemetriaIaDto> ObterResumoAsync(DateTime? inicio = null, DateTime? fim = null, CancellationToken ct = default);
        Task<List<LogUsoIADto>> ObterHistoricoAsync(int limit = 50, CancellationToken ct = default);
        Task<bool> LimparHistoricoAsync(CancellationToken ct = default);
    }

    public class RegistroChamadaIaInput
    {
        public string Funcionalidade { get; set; } = string.Empty;
        public string Operacao { get; set; } = "chat.completions";
        public string Modelo { get; set; } = "gemini-2.5-flash";
        public string OrigemMotor { get; set; } = "GEMINI_SYSTEM2_CLOUD"; // "GEMINI_SYSTEM2_CLOUD" ou "JEV_SYSTEM1_LOCAL"
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int QtdFerramentas { get; set; }
        public long LatenciaMs { get; set; }
        public bool Sucesso { get; set; } = true;
        public int? CodigoStatus { get; set; } = 200;
        public string? MensagemErro { get; set; }
    }

    public class LogUsoIADto
    {
        public Guid Id { get; set; }
        public DateTime DataHora { get; set; }
        public string Funcionalidade { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string OrigemMotor { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public int QtdFerramentas { get; set; }
        public long LatenciaMs { get; set; }
        public bool Sucesso { get; set; }
        public int? CodigoStatus { get; set; }
        public string? MensagemErro { get; set; }
        public decimal CustoEstimadoUSD { get; set; }
        public decimal CustoEconomizadoUSD { get; set; }
    }

    public class ResumoTelemetriaIaDto
    {
        public int TotalChamadas { get; set; }
        public int ChamadasComSucesso { get; set; }
        public int ChamadasComErro { get; set; }
        public double TaxaSucessoPercentual { get; set; }
        public int TotalChamadasGemini { get; set; }
        public int TotalChamadasJev { get; set; }

        public long TotalPromptTokens { get; set; }
        public long TotalCompletionTokens { get; set; }
        public long TotalTokens { get; set; }
        public int TotalFerramentas { get; set; }

        public double LatenciaMediaMs { get; set; }
        public double LatenciaP95Ms { get; set; }
        public double LatenciaMediaGeminiMs { get; set; }
        public double LatenciaMediaJevMs { get; set; }

        public decimal CustoTotalUSD { get; set; }
        public decimal CustoTotalBRL { get; set; }
        public decimal CustoPorRequisicaoSucessoUSD { get; set; }
        public decimal CustoPorRequisicaoSucessoBRL { get; set; }

        public decimal TotalEconomizadoJevUSD { get; set; }
        public decimal TotalEconomizadoJevBRL { get; set; }
        public long TokensEconomizadosJev { get; set; }

        public List<MetricaPorFuncionalidadeDto> DistribuicaoPorFuncionalidade { get; set; } = new();
        public List<EvolucaoDiariaTelemetriaDto> EvolucaoDiaria { get; set; } = new();
    }

    public class MetricaPorFuncionalidadeDto
    {
        public string Funcionalidade { get; set; } = string.Empty;
        public int Chamadas { get; set; }
        public long Tokens { get; set; }
        public decimal CustoUSD { get; set; }
        public double LatenciaMediaMs { get; set; }
        public double PorcentagemUso { get; set; }
    }

    public class EvolucaoDiariaTelemetriaDto
    {
        public string Data { get; set; } = string.Empty; // YYYY-MM-DD
        public int ChamadasGemini { get; set; }
        public int ChamadasJev { get; set; }
        public long Tokens { get; set; }
        public decimal CustoUSD { get; set; }
    }
}
