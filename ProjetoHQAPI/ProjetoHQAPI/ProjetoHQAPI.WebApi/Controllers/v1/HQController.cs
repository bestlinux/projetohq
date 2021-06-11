using ProjetoHQApi.Application.Features.HQs.Commands;
using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.Application.Features.Positions.Commands.CreatePosition;
using ProjetoHQApi.Application.Features.Positions.Commands.DeletePositionById;
using ProjetoHQApi.Application.Features.Positions.Commands.UpdatePosition;
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

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class HqController : BaseApiController
    {
        private static readonly string DIRETORIO_IMAGENS = @"D:\ProjetoHQS\Frases\";
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetHQQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        /// <summary>
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
        /// GET api/controller/5
        /// </summary>
        /// <param name="editora"></param>
        /// <returns></returns>
        [HttpGet("BuscaPorEditora/{editora}")]
        public async Task<IActionResult> Get(string editora)
        {
            return Ok(await Mediator.Send(new GetHQByEditoraQuery { Editora = editora }));
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
                string erro = ((ProjetoHQApi.Application.Exceptions.ValidationException)e).Errors[0];
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
            return Ok(await Mediator.Send(query));
        }

        [HttpGet("BuscaWeb/{titulo}/{editora}/{categoria}/{genero}/{status}/{formato}/{numeroEdicao?}/{anoLancamento?}")]
        public async Task<IActionResult> BuscaWeb(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
            return Ok(await Mediator.Send(new GetHQInWeb { Titulo = titulo, AnoLancamento = anoLancamento, NumeroEdicao = numeroEdicao, Editora = editora, Categoria = categoria, Genero = genero, Status = status, Formato = formato }));
        }

        [HttpPost]
        [Route("UploadImage")]
        public ObjectResult UploadImage()
        {
            var file = Request.Form.Files[0];

            string newPath = Path.Combine(DIRETORIO_IMAGENS);

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
            }

            return Ok(fileName);
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
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await Mediator.Send(command));
        }

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
