using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Features.Desejos.Commands;
using ProjetoHQApi.Application.Features.Desejos.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class DesejoController : BaseApiController
    {
        private readonly ILogger<DesejoController> _logger;

        public DesejoController(ILogger<DesejoController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetDesejoQuery filter)
        {
            try
            {
                return Ok(await Mediator.Send(filter));
            }
            catch (Exception e)
            {
                Application.Wrappers.Response<Guid> response = new();

                string erro;

                if (e.GetType() == typeof(ValidationException))
                    erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
                else
                    erro = e.StackTrace.ToString();

                response.Message = erro;
                response.Succeeded = false;
                _logger.LogError("Erro " + erro);
                return Ok(response);
            }
        }

        /// <summary>
        /// POST api/controller
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Desejo")]
        public async Task<IActionResult> Post(CreateDesejoCommand command)
        {
            Application.Wrappers.Response<Guid> response = new Application.Wrappers.Response<Guid>();
            try
            {
                response = await Mediator.Send(command);
            }
            catch (Exception e)
            {
                string erro;

                if (e.GetType() == typeof(ValidationException))
                    erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
                else
                    erro = e.StackTrace.ToString();

                response.Message = erro;
                response.Succeeded = false;
                return Ok(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Support Angular 11 CRUD story on Medium
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paged")]
        public async Task<IActionResult> Paged(PagedDesejoQuery query)
        {
            try
            {
                return Ok(await Mediator.Send(query));
            }
            catch (Exception e)
            {
                Application.Wrappers.Response<Guid> response = new();

                string erro;

                if (e.GetType() == typeof(ValidationException))
                    erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
                else
                    erro = e.StackTrace.ToString();

                response.Message = erro;
                response.Succeeded = false;
                _logger.LogError("Erro " + erro);
                return Ok(response);
            }
        }

        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return Ok(await Mediator.Send(new DeleteDesejoByIdCommand { Id = id }));
            }
            catch (Exception e)
            {
                Application.Wrappers.Response<Guid> response = new();

                string erro;

                if (e.GetType() == typeof(ValidationException))
                    erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
                else
                    erro = e.StackTrace.ToString();

                response.Message = erro;
                response.Succeeded = false;
                _logger.LogError("Erro " + erro);
                return Ok(response);
            }
        }
    }
}
