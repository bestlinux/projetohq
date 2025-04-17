using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Colecoes.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class ColecaoController(ILogger<ColecaoController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<ColecaoController> _logger = logger;
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
        public async Task<IActionResult> GetAll([FromQuery] GetColecaoQuery filter, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(filter, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Route("UploadImage")]
        public async Task<ObjectResult> UploadImageAsync(CancellationToken cancellationToken)
        {
            CreateColecaoCommand command = new();
            var file = Request.Form.Files[0];


            Request.Form.TryGetValue("Descricao", out StringValues Descricao);

            command.Descricao = Descricao[0];

            string newPath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_COLECAO);

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
                    file.CopyTo(stream);
                }

                command.Arquivo = fileName;
                await _mediator.Send(command, cancellationToken);
            }

            return Ok(fileName);
        }

        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
        {

            var result = await _mediator.Send(new DeleteColecaoByIdCommand { ColecaoId = id }, cancellationToken);
            return Ok(result);

        }
    }
}
