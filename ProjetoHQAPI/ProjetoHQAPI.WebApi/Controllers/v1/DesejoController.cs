using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Features.Desejos.Commands;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Desejos.Commands;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using ProjetoHQApi.Application.Constantes;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class DesejoController(ILogger<DesejoController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<DesejoController> _logger = logger;
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
        public async Task<IActionResult> GetAll([FromQuery] GetDesejoQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreateDesejoCommand command, CancellationToken cancellationToken)
        {

           var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }


        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteDesejoByIdCommand { DesejoId = id }, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// GET api/desejo/capa/{id}
        /// Carrega a imagem da capa a partir do ID/nome de arquivo em projetohqrepositorio\Desejos
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
            string fullPath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_DESEJOS, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                return PhysicalFile(fullPath, "image/jpeg");
            }

            // Se não encontrou e não tinha extensão .jpg, tenta adicionando .jpg
            if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                string pathWithJpg = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_DESEJOS, fileName + ".jpg");
                if (System.IO.File.Exists(pathWithJpg))
                {
                    return PhysicalFile(pathWithJpg, "image/jpeg");
                }
            }

            return NotFound();
        }
    }
}
