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
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {

            return Ok(await _mediator.Send(new GetHQByIdQuery { Id = id }, cancellationToken));
        }

        /// <summary>
        /// GET api/controller/5
        /// </summary>
        /// <param name="editora"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("BuscaPorEditora/{editora}")]
        public async Task<IActionResult> Get(string editora, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetHQByEditoraQuery { Editora = editora }, cancellationToken));
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateHQCommand[] command, CancellationToken cancellationToken)
        {
            var response = new Guid();

            foreach (CreateHQCommand commandHQ in command)
            {
                response = (Guid)await _mediator.Send(commandHQ, cancellationToken);
            }

            return Ok(response);
        }

        [HttpGet("BuscaWeb/{titulo}/{editora}/{categoria}/{genero}/{status}/{formato}/{numeroEdicao?}/{anoLancamento?}")]
        public async Task<IActionResult> BuscaWeb(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
           
           return Ok(await _mediator.Send(new GetHQInWeb { Titulo = titulo, AnoLancamento = anoLancamento, NumeroEdicao = numeroEdicao, Editora = editora, Categoria = categoria, Genero = genero, Status = status, Formato = formato }));
        }



        /// <summary>
        /// PUT api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateHQCommand command, CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await _mediator.Send(command, cancellationToken));

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
           return Ok(await _mediator.Send(new DeleteHQByIdCommand { Id = id }, cancellationToken));           
        }
    }
}
