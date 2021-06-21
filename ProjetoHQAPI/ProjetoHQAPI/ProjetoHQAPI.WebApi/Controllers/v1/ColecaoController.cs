using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ProjetoHQApi.Application.Features.Colecoes.Commands;
using ProjetoHQApi.Application.Features.Colecoes.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [Produces("application/json")]
    public class ColecaoController : BaseApiController
    {
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetColecaoQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }


        [HttpPost]
        [Route("Paged")]
        public async Task<IActionResult> Paged(PagedColecaoQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpPost]
        [Route("UploadImage")]
        public async Task<ObjectResult> UploadImageAsync()
        {
            CreateColecaoCommand command = new();
            var file = Request.Form.Files[0];


            Request.Form.TryGetValue("Descricao", out StringValues Descricao);

            command.Descricao = Descricao[0];

            string newPath = Path.Combine(Constantes.Constantes.GetDIRETORIO_IMAGENS_COLECAO());

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
                await Mediator.Send(command);
            }

            return Ok(fileName);
        }

        /// <summary>
        /// DELETE api/controller/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeleteColecaoByIdCommand { Id = id }));
        }
    }
}
