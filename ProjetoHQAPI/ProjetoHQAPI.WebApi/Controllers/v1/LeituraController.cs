using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.UseCases.Leituras.Commands;
using ProjetoHQApi.Application.UseCases.Leituras.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class LeituraController(ILogger<LeituraController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<LeituraController> _logger = logger;
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
        public async Task<IActionResult> GetAll([FromQuery] GetLeituraQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// <summary>
        /// GET: api/leitura/{id}
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetLeituraByIdQuery { Id = id }, cancellationToken);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        /// <summary>
        /// POST api/leitura
        /// </summary>
        [HttpPost]
        [Route("")]
        [Route("Leitura")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CreateLeituraCommand command, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// DELETE api/leitura/{id}
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new DeleteLeituraByIdCommand { Id = id }, cancellationToken));
        }

        /// <summary>
        /// PUT api/leitura/{id}
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put([FromRoute] Guid? id, [FromBody] UpdateLeituraCommand command, CancellationToken cancellationToken)
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                command.Id = id.Value;
            }

            if (command == null || command.Id == Guid.Empty)
            {
                return BadRequest("O ID da leitura é obrigatório.");
            }

            return Ok(await _mediator.Send(command, cancellationToken));
        }
    }
}
