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
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] GetFrasesQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }


        [HttpPost]
        [Route("UploadImage")]
        public async Task<ObjectResult> UploadImageAsync(CancellationToken cancellationToken)
        {
            CreateFraseCommand command = new();
            var file = Request.Form.Files[0];


            Request.Form.TryGetValue("NomeHQ", out StringValues NomeHQ);
            Request.Form.TryGetValue("Autor", out StringValues Autor);

            command.NomeHQ = NomeHQ[0];
            command.Autor = Autor[0];

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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new DeleteFraseByIdCommand { FraseId = id }, cancellationToken));
        }
    }
}
