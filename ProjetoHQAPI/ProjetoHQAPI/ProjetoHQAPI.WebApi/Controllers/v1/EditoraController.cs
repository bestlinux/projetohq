using ProjetoHQApi.Application.Features.Editoras.Commands;
using ProjetoHQApi.Application.Features.Editoras.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class EditoraController : BaseApiController
    {
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetEditoraQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetEditoraByIdQuery { Id = id }));
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEditoraCommand command)
        {
            Application.Wrappers.Response<Guid> response = new Application.Wrappers.Response<Guid>();
            try
            {
                response = await Mediator.Send(command);               
            }
            catch (Exception e)
            {
                string erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
                response.Message = erro;
                response.Succeeded = false;
                return Ok(response);
            }

            return Ok(response);
        }

       [HttpPost]
       [Route("Paged")]
       public async Task<IActionResult> Paged(PagedEditorasQuery query)
       {
          return Ok(await Mediator.Send(query));
       }

        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeleteEditoraByIdCommand { Id = id }));
        }

      /*/// <summary>
        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetHQByIdQuery { Id = id }));
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateHQCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Bulk insert fake data by specifying number of rows
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /*[HttpPost]
        [Route("AddMock")]
        public async Task<IActionResult> AddMock(InsertMockPositionCommand command)
        {
            return Ok(await Mediator.Send(command));
        }*/

        /// <summary>
        /// Support Angular 11 CRUD story on Medium
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /*[HttpPost]
        [Route("Paged")]
        public async Task<IActionResult> Paged(PagedHQsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("SearchWeb/{titulo}/{anoLancamento}")]
        public async Task<IActionResult> SearchWeb(string titulo, string anoLancamento)
        {
            return Ok(await Mediator.Send(new GetHQInWeb { Titulo = titulo }));
        }*/

        /// <summary>
        /// PUT api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /*[HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdatePositionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await Mediator.Send(command));
        }*/

        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /*[HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeletePositionByIdCommand { Id = id }));
        }*/
    }
}
