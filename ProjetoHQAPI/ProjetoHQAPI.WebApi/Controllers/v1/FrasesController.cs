using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.USeCase.Frases.Commands;
using ProjetoHQApi.Application.UseCases.Frases.Commands;
using ProjetoHQApi.Application.UseCases.Frases.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class FrasesController(ILogger<FrasesController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<FrasesController> _logger = logger;
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// GET: api/frases
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetFrasesQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// <summary>
        /// GET: api/frases/{id}
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetFraseByIdQuery { Id = id }, cancellationToken);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        /// <summary>
        /// POST: api/frases (JSON)
        /// </summary>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CreateFraseCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// POST: api/frases/UploadImage (Multipart/form-data)
        /// </summary>
        [HttpPost]
        [Route("UploadImage")]
        public async Task<IActionResult> UploadImageAsync(CancellationToken cancellationToken)
        {
            CreateFraseCommand command = new();
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }

            var file = Request.Form.Files[0];

            Request.Form.TryGetValue("NomeHQ", out StringValues NomeHQ);
            Request.Form.TryGetValue("Autor", out StringValues Autor);

            command.NomeHQ = NomeHQ.Count > 0 ? NomeHQ[0] : "";
            command.Autor = Autor.Count > 0 ? Autor[0] : "";

            string newPath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_FRASES);

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            string fileName = "";
            if (file.Length > 0)
            {
                fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string fullPath = Path.Combine(newPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                command.Arquivo = fileName;
                var id = await _mediator.Send(command, cancellationToken);
                return Ok(new { id, fileName });
            }

            return BadRequest("Arquivo vazio.");
        }

        /// <summary>
        /// PUT: api/frases/{id}
        /// </summary>
        [HttpPut("{id:guid}")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromRoute] Guid? id, [FromBody] UpdateFraseCommand command, CancellationToken cancellationToken)
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                command.Id = id.Value;
            }

            if (command == null || command.Id == Guid.Empty)
            {
                return BadRequest("O ID da frase é obrigatório.");
            }

            return Ok(await _mediator.Send(command, cancellationToken));
        }

        /// <summary>
        /// DELETE: api/frases/{id}
        /// </summary>
        [HttpDelete("{id:guid}")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new DeleteFraseByIdCommand { FraseId = id }, cancellationToken));
        }
    }
}
