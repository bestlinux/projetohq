using ProjetoHQApi.Application.Features.Editoras.Commands;
using ProjetoHQApi.Application.Features.Editoras.Queries;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class EditoraController : BaseApiController
    {
        private readonly ILogger<EditoraController> _logger;

        public EditoraController(ILogger<EditoraController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetEditoraQuery filter)
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

        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return Ok(await Mediator.Send(new GetEditoraByIdQuery { Id = id }));
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
        public async Task<IActionResult> Post(CreateEditoraCommand command)
        {
            try
            {
                //Read Configuration from appSettings
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
                    _logger.LogError("Erro ao incluir editora " + erro);
                    return Ok(response);
                }

                return Ok(response);
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

       [HttpPost]
       [Route("Paged")]
       public async Task<IActionResult> Paged(PagedEditorasQuery query)
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
                return Ok(await Mediator.Send(new DeleteEditoraByIdCommand { Id = id }));
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
