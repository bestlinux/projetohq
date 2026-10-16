using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface IJevEngineService
    {
        /// <summary>
        /// Calcula a similaridade probabilística composta (0 a 100%) entre duas HQs,
        /// aplicando Levenshtein, Jaro-Winkler, Token Set Ratio, pesos de edição e normalização canônica de editora.
        /// </summary>
        JevProbabilisticMatchResult CalcularSimilaridade(
            string tituloA, string? editoraA, int edicaoA,
            string tituloB, string? editoraB, int edicaoB);

        /// <summary>
        /// Busca as HQs mais semelhantes no acervo local para um dado título, editora e edição.
        /// </summary>
        Task<List<JevColecaoMatchDto>> BuscarSemelhantesColecaoAsync(
            string titulo, string? editora, int edicao, int maxResultados = 30, CancellationToken ct = default);

        /// <summary>
        /// Roteador do Sistema 1: Tenta resolver comandos em linguagem natural (CRUD)
        /// de forma heurística, rápida e determinística (<5ms). Retorna null se precisar do Sistema 2 (Gemini).
        /// </summary>
        Task<ComandoCrudPreviewResponse?> RotearComandoCrudSystem1Async(
            string comando, CancellationToken ct = default);

        /// <summary>
        /// Roteador do Sistema 1: Tenta resolver consultas diretas do Chatbot (estatísticas, listagem por autor/personagem, contagens)
        /// em <5ms sem chamar a API do Gemini. Retorna null se a pergunta exigir raciocínio profundo, narrativa ou sugestões livres.
        /// </summary>
        Task<ChatbotResponse?> RotearChatbotSystem1Async(
            ChatbotRequest request, CancellationToken ct = default);
    }

    public class JevProbabilisticMatchResult
    {
        public double ScoreGeral { get; set; } // 0 a 100
        public double ScoreTitulo { get; set; } // 0 a 100
        public double ScoreEditora { get; set; } // 0 a 100
        public double ScoreEdicao { get; set; } // 0 a 100
        public string Classificacao { get; set; } = string.Empty; // "Duplicata Confirmada", "Mesma Série (Outra Edição)", "Possível Variante / Reimpressão", "Distinto"
        public string Motivo { get; set; } = string.Empty;
        public bool IsProvavelDuplicata => ScoreGeral >= 85.0;
    }

    public class JevColecaoMatchDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Editora { get; set; } = string.Empty;
        public int NumeroEdicao { get; set; }
        public int Lido { get; set; }
        public string? Imagem { get; set; }
        public double ScoreGeral { get; set; }
        public double ScoreTitulo { get; set; }
        public double ScoreEditora { get; set; }
        public double ScoreEdicao { get; set; }
        public string Classificacao { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class JevBuscaRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Editora { get; set; }
        public int Edicao { get; set; } = 1;
        public int? MaxResultados { get; set; } = 25;
    }
}
