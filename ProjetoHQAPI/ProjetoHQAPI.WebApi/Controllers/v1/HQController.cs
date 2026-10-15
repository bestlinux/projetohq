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
using System.Threading.Tasks;
using ProjetoHQApi.Application.Features.Desejos.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ProjetoHQApi.Application.UseCases.HQs.Queries;
using ProjetoHQApi.Application.UseCases.HQs.Commands;
using ProjetoHQApi.Application.Constantes;

namespace ProjetoHQApi.WebApi.Controllers.v1
{

    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class HqController(ILogger<HqController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<HqController> _logger = logger;
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetHQQuery filter, CancellationToken cancellationToken)
        {

            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// <summary>
        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <summary>
        /// GET api/hq/{id}
        /// </summary>
        [HttpGet("{id:guid}")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetHQByIdQuery { Id = id }, cancellationToken);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        /// <summary>
        /// GET api/hq/BuscaPorEditora/{editora}
        /// </summary>
        [HttpGet("BuscaPorEditora/{editora}")]
        public async Task<IActionResult> Get(string editora, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQByEditoraQuery { Editora = editora }, cancellationToken));
        }

        /// <summary>
        /// POST api/hq (aceita tanto objeto único quanto array de comandos)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                return Ok(lastId);
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

        [HttpGet("BuscaWeb/{titulo}/{editora}/{categoria}/{genero}/{status}/{formato}/{numeroEdicao?}/{anoLancamento?}")]
        public async Task<IActionResult> BuscaWeb(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
            return Ok(await _mediator.Send(new GetHQInWeb { Titulo = titulo, AnoLancamento = anoLancamento, NumeroEdicao = numeroEdicao, Editora = editora, Categoria = categoria, Genero = genero, Status = status, Formato = formato }));
        }

        /// <summary>
        /// PUT api/hq/{id}
        /// </summary>
        [HttpPut("{id:guid}")]
        [HttpPut("{id}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            return Ok(await _mediator.Send(command, cancellationToken));
        }

        /// <summary>
        /// DELETE api/hq/{id}
        /// </summary>
        [HttpDelete("{id:guid}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new DeleteHQByIdCommand { Id = id }, cancellationToken));           
        }

        /// <summary>
        /// GET api/hq/capa/{id}
        /// Carrega a imagem da capa a partir do ID/nome de arquivo em projetohqrepositorio\Capas
        /// </summary>
        [HttpGet("capa/{id}")]
        [AllowAnonymous]
        public IActionResult GetCapa(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("ID da capa é obrigatório");
            }

            // Remove qualquer tentativa de path traversal
            string fileName = Path.GetFileName(id);

            // Verifica primeiro com o nome exato
            string fullPath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                return PhysicalFile(fullPath, "image/jpeg");
            }

            // Se não encontrou e não tinha extensão .jpg, tenta adicionando .jpg
            if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                string pathWithJpg = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, fileName + ".jpg");
                if (System.IO.File.Exists(pathWithJpg))
                {
                    return PhysicalFile(pathWithJpg, "image/jpeg");
                }
            }

            return NotFound();
        }
    }
}
