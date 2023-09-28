using ProjetoHQApi.Application.Features.HQs.Commands;
using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.WebApi.Models;
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

namespace ProjetoHQApi.WebApi.Controllers.v1
{

    [ApiVersion("1.0")]
    public class HqController : BaseApiController
    {
        private readonly ILogger<HqController> _logger;

        public HqController(ILogger<HqController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetHQQuery filter)
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
        /// GET api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return Ok(await Mediator.Send(new GetHQByIdQuery { Id = id }));
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
        /// GET api/controller/5
        /// </summary>
        /// <param name="editora"></param>
        /// <returns></returns>
        [HttpGet("BuscaPorEditora/{editora}")]
        public async Task<IActionResult> Get(string editora)
        {
            try
            {
                return Ok(await Mediator.Send(new GetHQByEditoraQuery { Editora = editora }));
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
        public async Task<IActionResult> Post(CreateHQCommand[] command)
        {
            Application.Wrappers.Response<Guid> response = new Application.Wrappers.Response<Guid>();
            try
            {
                foreach (CreateHQCommand commandHQ in command)
                {
                    response = await Mediator.Send(commandHQ);                   
                }
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
        public async Task<IActionResult> Paged(PagedHQsQuery query)
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

        [HttpGet("BuscaWeb/{titulo}/{editora}/{categoria}/{genero}/{status}/{formato}/{numeroEdicao?}/{anoLancamento?}")]
        public async Task<IActionResult> BuscaWeb(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
            try
            {
                return Ok(await Mediator.Send(new GetHQInWeb { Titulo = titulo, AnoLancamento = anoLancamento, NumeroEdicao = numeroEdicao, Editora = editora, Categoria = categoria, Genero = genero, Status = status, Formato = formato }));
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

        [HttpGet]
        [Route("BuscaAvancada/{categoria=0}/{genero=0}/{status=0}/{formato=0}/{lido=0}/{numeroEdicao=0}/{anoLancamento=null}/{titulo=null}/{roteiro=null}/{personagens=null}/{editora=null}")]
        public async Task<IActionResult> BuscaAvancada(int categoria, int genero, int status, int formato, int lido, int numeroEdicao, string anoLancamento, string titulo, string roteiro, string personagens, string editora)
        {
            try
            {
                return Ok(await Mediator.Send(new GetHQAdvancedSearchQuery { Titulo = titulo, AnoLancamento = anoLancamento, NumeroEdicao = numeroEdicao, Editora = editora, Categoria = categoria, Genero = genero, Status = status, Formato = formato, Lido = lido, Roteiro = roteiro, Personagens = personagens }));
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
        public async Task<IActionResult> Put(Guid id, UpdateHQCommand command)
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
                return Ok(await Mediator.Send(new DeleteHQByIdCommand { Id = id }));
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
