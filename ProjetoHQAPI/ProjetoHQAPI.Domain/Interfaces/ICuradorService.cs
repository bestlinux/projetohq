using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface ICuradorService
    {
        Task<OrdemLeituraResponse> GerarOrdemLeituraAsync(OrdemLeituraRequest request, CancellationToken cancellationToken = default);
        Task<DnaColecaoResponse> AnalisarDnaColecaoEGapsAsync(CancellationToken cancellationToken = default);
        Task<RecapNarrativoResponse> GerarRecapNarrativoAsync(RecapNarrativoRequest request, CancellationToken cancellationToken = default);
        Task<QuizAcervoResponse> GerarQuizAcervoAsync(int quantidadePerguntas = 5, CancellationToken cancellationToken = default);
        Task<ChatbotResponse> ConsultarChatbotColecaoAsync(ChatbotRequest request, CancellationToken cancellationToken = default);
        Task<ComandoCrudPreviewResponse> ProcessarComandoCrudAssistidoAsync(string comando, CancellationToken cancellationToken = default);
        Task<ComandoCrudExecutarResult> ExecutarComandoCrudAsync(ComandoCrudExecutarRequest request, CancellationToken cancellationToken = default);
        Task<ImportacaoLotePreviewResponse> GerarImportacaoLoteAsync(string textoLote, CancellationToken cancellationToken = default);
        Task<ImportacaoLoteSalvarResult> SalvarImportacaoLoteAsync(ImportacaoLoteSalvarRequest request, CancellationToken cancellationToken = default);
    }

    #region DTOs - Ordem de Leitura
    public class OrdemLeituraRequest
    {
        public string Query { get; set; } = string.Empty;
        public string? Universo { get; set; }
    }

    public class OrdemLeituraResponse
    {
        public string Saga { get; set; } = string.Empty;
        public string Universo { get; set; } = string.Empty;
        public string DescricaoGeral { get; set; } = string.Empty;
        public List<ItemOrdemLeituraDto> Itens { get; set; } = new();
        public int TotalItensNaEstante { get; set; }
        public int TotalItensAusentes { get; set; }
    }

    public class ItemOrdemLeituraDto
    {
        public int Ordem { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string EditoraRecomendada { get; set; } = string.Empty;
        public int NumeroEdicao { get; set; }
        public string Importancia { get; set; } = "Essencial"; // "Essencial", "Prelúdio", "Tie-in", "Epílogo"
        public string SinopseCurta { get; set; } = string.Empty;
        public bool NaEstante { get; set; }
        public Guid? HqId { get; set; }
        public bool Lido { get; set; }
    }
    #endregion

    #region DTOs - DNA do Acervo & Gaps
    public class DnaColecaoResponse
    {
        public string Arquetipo { get; set; } = string.Empty;
        public string SubtituloArquetipo { get; set; } = string.Empty;
        public string DescricaoArquetipo { get; set; } = string.Empty;
        public int TotalHqs { get; set; }
        public int TotalLidas { get; set; }
        public double PorcentagemLidas { get; set; }
        public List<MetricaPercentualDto> DistribuicaoEditoras { get; set; } = new();
        public List<MetricaPercentualDto> DistribuicaoGeneros { get; set; } = new();
        public List<GapSerieDto> GapsDetectados { get; set; } = new();
        public List<RecomendacaoAquisicaoDto> RecomendacoesProximasAquisicoes { get; set; } = new();
        public List<string> PontosFortesColecao { get; set; } = new();
    }

    public class MetricaPercentualDto
    {
        public string Nome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public double Porcentagem { get; set; }
    }

    public class GapSerieDto
    {
        public string Serie { get; set; } = string.Empty;
        public string Editora { get; set; } = string.Empty;
        public List<int> EdicoesPossuidas { get; set; } = new();
        public List<int> EdicoesFaltantes { get; set; } = new();
        public string Urgencia { get; set; } = "Média"; // Alta, Média, Baixa
        public string MotivoGap { get; set; } = string.Empty;
    }

    public class RecomendacaoAquisicaoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Editora { get; set; } = string.Empty;
        public int NumeroEdicao { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string MatchScore { get; set; } = "95%";
    }
    #endregion

    #region DTOs - Storyteller / Recap Narrativo
    public class RecapNarrativoRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public int? NumeroEdicao { get; set; }
        public string? Editora { get; set; }
    }

    public class RecapNarrativoResponse
    {
        public string Titulo { get; set; } = string.Empty;
        public int NumeroEdicao { get; set; }
        public string ArcoNarrativo { get; set; } = string.Empty;
        public string PreviouslyOnText { get; set; } = string.Empty;
        public List<string> PersonagensChave { get; set; } = new();
        public List<string> EventosPrecedentes { get; set; } = new();
        public string ClimaOuTom { get; set; } = string.Empty;
    }
    #endregion

    #region DTOs - Trivia & Quiz
    public class QuizAcervoResponse
    {
        public List<PerguntaQuizDto> Perguntas { get; set; } = new();
    }

    public class PerguntaQuizDto
    {
        public int Id { get; set; }
        public string Pergunta { get; set; } = string.Empty;
        public List<string> Opcoes { get; set; } = new();
        public int RespostaCorretaIndex { get; set; }
        public string Explicacao { get; set; } = string.Empty;
        public string HqReferencia { get; set; } = string.Empty;
    }
    #endregion

    #region DTOs - Chatbot com RAG em Memória
    public class ChatbotRequest
    {
        public string Mensagem { get; set; } = string.Empty;
        public List<ChatMessageDto> Historico { get; set; } = new();
    }

    public class ChatMessageDto
    {
        public string Remetente { get; set; } = "user"; // "user" ou "model"
        public string Texto { get; set; } = string.Empty;
    }

    public class ChatbotResponse
    {
        public string Resposta { get; set; } = string.Empty;
        public List<string> HqsMencionadas { get; set; } = new();
        public List<string> SugestoesPerguntas { get; set; } = new();
        public string OrigemMotor { get; set; } = "GEMINI_SYSTEM2_CLOUD";
        public long TempoProcessamentoMs { get; set; }
    }
    #endregion

    #region DTOs - Comandos CRUD em Linguagem Natural
    public class ComandoCrudPreviewResponse
    {
        public string ComandoOriginal { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty; // "marcar_lido", "desmarcar_lido", "criar_hq", "criar_desejo", "excluir_hq"
        public string InterpretacaoHumana { get; set; } = string.Empty;
        public string EntidadeAlvo { get; set; } = "HQ";
        public Guid? HqIdAlvo { get; set; }
        public string? TituloAlvo { get; set; }
        public string? EditoraAlvo { get; set; }
        public int? EdicaoAlvo { get; set; }
        public string? ParametrosModificadosJson { get; set; }
        public bool RequerConfirmacao { get; set; } = true;
        public string OrigemMotor { get; set; } = "GEMINI_SYSTEM2_CLOUD";
        public long TempoProcessamentoMs { get; set; }
    }

    public class ComandoCrudExecutarRequest
    {
        public string Acao { get; set; } = string.Empty;
        public Guid? HqIdAlvo { get; set; }
        public string? Titulo { get; set; }
        public string? Editora { get; set; }
        public int NumeroEdicao { get; set; } = 1;
        public string? Preco { get; set; }
        public int? Lido { get; set; }
    }

    public class ComandoCrudExecutarResult
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public Guid? IdRegistroAfetado { get; set; }
    }
    #endregion

    #region DTOs - Importação em Lote Assistida
    public class ImportacaoLotePreviewResponse
    {
        public int TotalDetectados { get; set; }
        public List<ItemLotePreviewDto> Itens { get; set; } = new();
    }

    public class ItemLotePreviewDto
    {
        public int IdTemp { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Editora { get; set; } = string.Empty;
        public int NumeroEdicao { get; set; }
        public string? AnoLancamento { get; set; }
        public int Categoria { get; set; } = 1;
        public int Genero { get; set; } = 1;
        public string PrecoSugerido { get; set; } = "R$ 0,00";
        public bool JaExisteNaColecao { get; set; }
        public bool Selecionado { get; set; } = true;
    }

    public class ImportacaoLoteSalvarRequest
    {
        public string Destino { get; set; } = "colecao"; // "colecao" ou "desejos"
        public List<ItemLotePreviewDto> Itens { get; set; } = new();
    }

    public class ImportacaoLoteSalvarResult
    {
        public int TotalSalvos { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
    #endregion
}
