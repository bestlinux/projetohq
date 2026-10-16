using ProjetoHQApi.Domain.Common;
using System;

namespace ProjetoHQApi.Domain.Entities
{
    public class LogUsoIA : Entity
    {
        public Guid Id { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
        public string Funcionalidade { get; set; } = string.Empty;
        public string Operacao { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string OrigemMotor { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public int QtdFerramentas { get; set; }
        public long LatenciaMs { get; set; }
        public bool Sucesso { get; set; } = true;
        public int? CodigoStatus { get; set; }
        public string? MensagemErro { get; set; }
        public decimal CustoEstimadoUSD { get; set; }
        public decimal CustoEconomizadoUSD { get; set; }
    }
}
