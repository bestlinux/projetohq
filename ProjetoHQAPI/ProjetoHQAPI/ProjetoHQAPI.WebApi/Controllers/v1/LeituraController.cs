using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Features.Leituras.Commands;
using ProjetoHQApi.Application.Features.Leituras.Queries;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class LeituraController : BaseApiController
    {
        private readonly ILogger<LeituraController> _logger;

        public LeituraController(ILogger<LeituraController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetLeituraQuery filter)
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
        [Route("Leitura")]
        public async Task<IActionResult> Post(CreateLeituraCommand command)
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
                Log.Fatal("Erro ao incluir a hq na lista de leitura " + e.StackTrace);
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
        public async Task<IActionResult> Paged(PagedLeituraQuery query)
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
                return Ok(await Mediator.Send(new DeleteLeituraByIdCommand { Id = id }));
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
        /// PUT api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateLeituraCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest();
                }
                return Ok(await Mediator.Send(command));
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
