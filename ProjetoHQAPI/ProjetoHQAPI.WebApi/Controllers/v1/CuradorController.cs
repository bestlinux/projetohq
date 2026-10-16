using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    public class CuradorController : ControllerBase
    {
        private readonly ICuradorService _curadorService;
        private readonly IJevEngineService _jevEngine;
        private readonly ITelemetriaIaService _telemetria;

        public CuradorController(
            ICuradorService curadorService, 
            IJevEngineService jevEngine,
            ITelemetriaIaService telemetria)
        {
            _curadorService = curadorService;
            _jevEngine = jevEngine;
            _telemetria = telemetria;
        }

        /// <summary>
        /// 1. Constrói a cronologia ideal de leitura para qualquer saga, personagem ou universo, cruzando com o acervo real.
        /// </summary>
        [HttpPost("ordem-leitura")]
        public async Task<IActionResult> GerarOrdemLeitura([FromBody] OrdemLeituraRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return BadRequest("O termo ou nome da saga é obrigatório.");
            }

            var resultado = await _curadorService.GerarOrdemLeituraAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 2. Detetive de Coleção & DNA do Acervo: define arquétipo do colecionador, detecta lacunas (gaps) sequenciais e recomenda próximas compras.
        /// </summary>
        [HttpGet("dna-gaps")]
        public async Task<IActionResult> AnalisarDnaEGaps(CancellationToken cancellationToken)
        {
            var resultado = await _curadorService.AnalisarDnaColecaoEGapsAsync(cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 3. Storyteller / Aquecimento Narrativo Imersivo: narrativa 'Previously on...' contextualizando arcos e acontecimentos anteriores.
        /// </summary>
        [HttpPost("recap")]
        public async Task<IActionResult> GerarRecap([FromBody] RecapNarrativoRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Titulo))
            {
                return BadRequest("O título da HQ é obrigatório.");
            }

            var resultado = await _curadorService.GerarRecapNarrativoAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 4. Trivia & Quiz Interativo do Acervo: perguntas e respostas dinâmicas baseadas estritamente nas HQs da estante.
        /// </summary>
        [HttpGet("quiz")]
        [HttpPost("quiz")]
        public async Task<IActionResult> GerarQuiz([FromQuery] int quantidade = 5, CancellationToken cancellationToken = default)
        {
            var resultado = await _curadorService.GerarQuizAcervoAsync(quantidade, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 5. Curador Virtual com Roteador Híbrido:
        /// - Sistema 1 (JEV Engine Local): Responde em ~2ms consultas determinísticas e estatísticas sem chamada à nuvem.
        /// - Sistema 2 (Gemini LLM): Acionado para perguntas profundas, culturais ou criativas.
        /// </summary>
        [HttpPost("chatbot")]
        public async Task<IActionResult> ConsultarChatbot([FromBody] ChatbotRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Mensagem))
            {
                return BadRequest("A mensagem é obrigatória.");
            }

            // 1. Roteamento Rápido: Sistema 1 Local (JEV Engine)
            var respostaJev = await _jevEngine.RotearChatbotSystem1Async(request, cancellationToken);
            if (respostaJev != null)
            {
                return Ok(respostaJev);
            }

            // 2. Sistema 2 Cognitivo (Gemini LLM Cloud)
            var resultado = await _curadorService.ConsultarChatbotColecaoAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 6. Assistente de Comandos CRUD:
        /// - Sistema 1 (JEV Engine Local): Intercepta comandos de marcar/desmarcar/excluir em ~2ms.
        /// - Sistema 2 (Gemini LLM): Processa comandos complexos ou ambíguos.
        /// </summary>
        [HttpPost("comando-crud/preview")]
        public async Task<IActionResult> PreviewComandoCrud([FromBody] ComandoCrudInput input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input?.Comando))
            {
                return BadRequest("O comando é obrigatório.");
            }

            // 1. Roteamento Rápido: Sistema 1 Local (JEV Engine)
            var previewJev = await _jevEngine.RotearComandoCrudSystem1Async(input.Comando, cancellationToken);
            if (previewJev != null)
            {
                return Ok(previewJev);
            }

            // 2. Sistema 2 Cognitivo (Gemini LLM Cloud)
            var resultado = await _curadorService.ProcessarComandoCrudAssistidoAsync(input.Comando, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 6b. Execução confirmada da ação do comando CRUD assistido no banco de dados.
        /// </summary>
        [HttpPost("comando-crud/executar")]
        public async Task<IActionResult> ExecutarComandoCrud([FromBody] ComandoCrudExecutarRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _curadorService.ExecutarComandoCrudAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 7. Importação em Lote Assistida: interpreta listas desestruturadas e intervalos numéricos.
        /// </summary>
        [HttpPost("importacao-lote/preview")]
        public async Task<IActionResult> PreviewImportacaoLote([FromBody] ImportacaoLoteInput input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input?.TextoLote))
            {
                return BadRequest("O texto do lote é obrigatório.");
            }

            var resultado = await _curadorService.GerarImportacaoLoteAsync(input.TextoLote, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 7b. Salva os quadrinhos do lote selecionados na coleção ou na lista de desejos.
        /// </summary>
        [HttpPost("importacao-lote/salvar")]
        public async Task<IActionResult> SalvarImportacaoLote([FromBody] ImportacaoLoteSalvarRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _curadorService.SalvarImportacaoLoteAsync(request, cancellationToken);
            return Ok(resultado);
        }

        /// <summary>
        /// 8. Observabilidade & Métricas de IA (OpenTelemetry):
        /// Retorna totais de chamadas, tokens, latência P95, custo USD/BRL, custo por requisição bem-sucedida e economia do JEV Engine.
        /// </summary>
        [HttpGet("telemetria/resumo")]
        public async Task<IActionResult> ObterResumoTelemetria([FromQuery] DateTime? inicio = null, [FromQuery] DateTime? fim = null, CancellationToken cancellationToken = default)
        {
            var resumo = await _telemetria.ObterResumoAsync(inicio, fim, cancellationToken);
            return Ok(resumo);
        }

        /// <summary>
        /// 8b. Histórico recente de auditoria de chamadas de IA gravadas via OpenTelemetry GenAI Conventions.
        /// </summary>
        [HttpGet("telemetria/historico")]
        public async Task<IActionResult> ObterHistoricoTelemetria([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            var historico = await _telemetria.ObterHistoricoAsync(limit, cancellationToken);
            return Ok(historico);
        }

        /// <summary>
        /// 8c. Limpa o histórico de auditoria de IA caso o usuário deseje reiniciar a contagem.
        /// </summary>
        [HttpPost("telemetria/limpar")]
        public async Task<IActionResult> LimparHistoricoTelemetria(CancellationToken cancellationToken = default)
        {
            var ok = await _telemetria.LimparHistoricoAsync(cancellationToken);
            return Ok(new { sucesso = ok, mensagem = "Histórico de telemetria reiniciado com sucesso." });
        }
    }

    public class ComandoCrudInput
    {
        public string Comando { get; set; } = string.Empty;
    }

    public class ImportacaoLoteInput
    {
        public string TextoLote { get; set; } = string.Empty;
    }
}
