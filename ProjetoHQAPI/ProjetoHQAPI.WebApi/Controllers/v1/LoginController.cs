using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Features.Login.Queries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    public class LoginController : BaseApiController
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// GET api/controller/5
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="senha"></param>
        /// <returns></returns>
        [HttpGet("ValidaUsuario/{usuario}/{senha}")]
        public async Task<IActionResult> Get(string usuario, string senha)
        {
            try
            {
                return Ok(await Mediator.Send(new GetLoginQuery { Usuario = usuario, Senha = senha }));
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
