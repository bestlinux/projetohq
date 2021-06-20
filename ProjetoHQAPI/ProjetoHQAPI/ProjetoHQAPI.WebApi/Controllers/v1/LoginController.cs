using Microsoft.AspNetCore.Mvc;
using ProjetoHQApi.Application.Features.Login.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    public class LoginController : BaseApiController
    {
		/// <summary>
		/// GET api/controller/5
		/// </summary>
		/// <param name="usuario"></param>
		/// <param name="senha"></param>
		/// <returns></returns>
		[HttpGet("ValidaUsuario/{usuario}/{senha}")]
        public async Task<IActionResult> Get(string usuario, string senha)
        {
            return Ok(await Mediator.Send(new GetLoginQuery { Usuario = usuario, Senha = senha }));
        }

    }
}
