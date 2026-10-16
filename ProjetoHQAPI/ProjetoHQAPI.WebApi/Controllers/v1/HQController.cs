using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.HQs.Queries;
using ProjetoHQApi.Application.UseCases.HQs.Commands;
using ProjetoHQApi.Application.Constantes;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    /// <summary>
    /// Gerenciamento completo de HQs (Quadrinhos)
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class HqController(
        ILogger<HqController> logger,
        IMediator mediator,
        IHQRepository hqRepository,
        IUnitOfWork unitOfWork
    ) : ControllerBase
    {
        private readonly ILogger<HqController> _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        /// Obtém lista de HQs com filtros opcionais (título, editora, ano, status lido, paginação e ordenação)
        /// </summary>
        /// <param name="filter">Parâmetros de filtro</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de HQs encontradas</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetHQQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// <summary>
        /// Obtém uma HQ específica pelo seu ID (GUID)
        /// </summary>
        /// <param name="id">ID da HQ</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados completos da HQ</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetHQByIdQuery { Id = id }, cancellationToken);
            if (response == null)
            {
                return NotFound($"HQ com ID {id} não foi encontrada.");
            }
            return Ok(response);
        }

        /// <summary>
        /// Obtém estatísticas consolidadas da coleção (totais, lidas, não lidas, editoras, páginas e valor estimado)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resumo estatístico da coleção de quadrinhos</returns>
        [HttpGet("estatisticas")]
        [ProducesResponseType(typeof(HQEstatisticasResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEstatisticas(CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQEstatisticasQuery(), cancellationToken));
        }

        /// <summary>
        /// Realiza busca rápida por termo nos campos título, editora, personagens e licenciador
        /// </summary>
        /// <param name="termo">Termo a ser buscado</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de HQs que contêm o termo</returns>
        [HttpGet("busca/{termo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Busca(string termo, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQQuery { Search = termo }, cancellationToken));
        }

        /// <summary>
        /// Retorna todas as HQs publicadas por uma editora específica
        /// </summary>
        /// <param name="editora">Nome da editora</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de HQs da editora</returns>
        [HttpGet("editora/{editora}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEditora(string editora, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQQuery { Editora = editora }, cancellationToken));
        }

        /// <summary>
        /// Verifica se existem HQs cadastradas para a editora especificada
        /// </summary>
        /// <param name="editora">Nome da editora</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Verdadeiro se existir ao menos uma HQ da editora</returns>
        [HttpGet("BuscaPorEditora/{editora}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> BuscaPorEditora(string editora, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQByEditoraQuery { Editora = editora }, cancellationToken));
        }

        /// <summary>
        /// Cria uma nova HQ ou importa um lote de HQs
        /// </summary>
        /// <param name="payload">Objeto único ou array de objetos CreateHQCommand</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>ID gerado da HQ</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] System.Text.Json.JsonElement payload, CancellationToken cancellationToken)
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (payload.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var commands = System.Text.Json.JsonSerializer.Deserialize<CreateHQCommand[]>(payload.GetRawText(), options);
                Guid lastId = Guid.Empty;
                if (commands != null)
                {
                    foreach (var cmd in commands)
                    {
                        var res = await _mediator.Send(cmd, cancellationToken);
                        if (res.HasValue) lastId = res.Value;
                    }
                }
                return Ok(new { success = true, lastId, total = commands?.Length ?? 0 });
            }
            else
            {
                var command = System.Text.Json.JsonSerializer.Deserialize<CreateHQCommand>(payload.GetRawText(), options);
                if (command == null)
                {
                    return BadRequest("Dados inválidos");
                }
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
        }

        /// <summary>
        /// Atualiza uma HQ existente
        /// </summary>
        /// <param name="id">ID da HQ (opcional na rota se vier no corpo)</param>
        /// <param name="command">Dados atualizados da HQ</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>ID da HQ atualizada</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Put([FromRoute] Guid? id, [FromBody] UpdateHQCommand command, CancellationToken cancellationToken)
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                command.Id = id.Value;
            }

            if (command == null || command.Id == Guid.Empty)
            {
                return BadRequest("O ID da HQ é obrigatório.");
            }

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.HasValue)
            {
                return NotFound($"HQ com ID {command.Id} não encontrada.");
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Alterna ou define rapidamente o status de leitura da HQ (Lido / Não Lido)
        /// </summary>
        /// <param name="id">ID da HQ</param>
        /// <param name="lido">Novo valor de status (opcional: 1 para lido, 0 para não lido; se omitido, inverte o status atual)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado da alteração</returns>
        [HttpPatch("{id:guid}/lido")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleLido(Guid id, [FromQuery] int? lido, CancellationToken cancellationToken)
        {
            var success = await _mediator.Send(new ToggleHQLidoCommand { Id = id, Lido = lido }, cancellationToken);
            if (!success)
            {
                return NotFound($"HQ com ID {id} não encontrada.");
            }
            return Ok(new { id, success = true, lido });
        }

        /// <summary>
        /// Realiza o upload de imagem de capa para a HQ
        /// </summary>
        /// <param name="id">ID da HQ</param>
        /// <param name="file">Arquivo de imagem (JPEG, PNG ou WEBP)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Informações da capa salva</returns>
        [HttpPost("{id:guid}/capa")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCapa(Guid id, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }

            var hq = await _hqRepository.GetByIdAsync(id);
            if (hq == null)
            {
                return NotFound($"HQ com ID {id} não encontrada.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest("Formato de arquivo inválido. Suportados: JPG, PNG, WEBP.");
            }

            if (!Directory.Exists(ConstantesProjetoHQ.DIRETORIO_IMAGENS))
            {
                Directory.CreateDirectory(ConstantesProjetoHQ.DIRETORIO_IMAGENS);
            }

            var fileName = $"{id}.jpg";
            var filePath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            hq.Capa = fileName;
            await _hqRepository.UpdateAsync(hq);
            await _unitOfWork.Commit(cancellationToken);

            return Ok(new
            {
                id = hq.Id,
                capa = fileName,
                url = $"/capas/{fileName}"
            });
        }

        /// <summary>
        /// Exclui uma HQ pelo ID
        /// </summary>
        /// <param name="id">ID da HQ</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Sucesso da exclusão</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new DeleteHQByIdCommand { Id = id }, cancellationToken));
        }

        /// <summary>
        /// Obtém a imagem física da capa de uma HQ armazenada no repositório local
        /// </summary>
        /// <param name="id">ID ou nome do arquivo da capa</param>
        /// <returns>Arquivo físico da imagem</returns>
        [HttpGet("capa/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCapa(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("ID da capa é obrigatório.");
            }

            string fileName = Path.GetFileName(id);

            // Verifica com o nome exato
            string fullPath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                return PhysicalFile(fullPath, "image/jpeg");
            }

            // Tenta com extensão .jpg se não tiver
            if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                string pathWithJpg = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, fileName + ".jpg");
                if (System.IO.File.Exists(pathWithJpg))
                {
                    return PhysicalFile(pathWithJpg, "image/jpeg");
                }
            }

            return NotFound("Imagem de capa não encontrada.");
        }

        /// <summary>
        /// Realiza busca avançada no site Guia dos Quadrinhos via web scraping
        /// </summary>
        [HttpGet("BuscaWeb/{titulo}/{editora}/{categoria}/{genero}/{status}/{formato}/{numeroEdicao?}/{anoLancamento?}")]
        public async Task<IActionResult> BuscaWeb(
            string titulo,
            string editora,
            int categoria,
            int genero,
            int status,
            int formato,
            int numeroEdicao = 0,
            string anoLancamento = null)
        {
            return Ok(await _mediator.Send(new GetHQInWeb
            {
                Titulo = titulo,
                AnoLancamento = anoLancamento,
                NumeroEdicao = numeroEdicao,
                Editora = editora,
                Categoria = categoria,
                Genero = genero,
                Status = status,
                Formato = formato
            }));
        }
    }
}
