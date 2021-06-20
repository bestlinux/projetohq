using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ProjetoHQApi.Application.Features.Frases.Commands;
using ProjetoHQApi.Application.Features.Frases.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
	[Produces("application/json")]
	public class FrasesController : BaseApiController
	{
        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetFrasesQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }


        [HttpPost]
        [Route("Paged")]
        public async Task<IActionResult> Paged(PagedFrasesQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpPost]
        [Route("UploadImage")]
        public async Task<ObjectResult> UploadImageAsync()
        {
            CreateFraseCommand command = new();
            var file = Request.Form.Files[0];


			Request.Form.TryGetValue("NomeHQ", out StringValues NomeHQ);
			Request.Form.TryGetValue("Autor", out StringValues Autor);

            command.NomeHQ = NomeHQ[0];
            command.Autor = Autor[0];

            string newPath = Path.Combine(Constantes.Constantes.GetDIRETORIO_IMAGENS_FRASES());

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
            return Ok(await Mediator.Send(new DeleteFraseByIdCommand { Id = id }));
        }
    }
}
