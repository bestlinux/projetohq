using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;
using MediatR;
using ProjetoHQApi.Application.UseCases.Editoras.Commands;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class EditoraController(ILogger<EditoraController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<EditoraController> _logger = logger;
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
        public async Task<IActionResult> GetAll([FromQuery] GetEditoraQuery filter, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(filter, cancellationToken));
        }

        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetEditoraByIdQuery { Id = id }, cancellationToken));
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEditoraCommand command, CancellationToken cancellationToken)
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {           
           return Ok(await _mediator.Send(new DeleteEditoraByIdCommand { EditoraId = id }, cancellationToken));          
        }
    }
}
