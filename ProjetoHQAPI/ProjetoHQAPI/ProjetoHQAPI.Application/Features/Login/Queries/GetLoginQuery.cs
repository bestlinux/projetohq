using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Login.Queries
{	
    public class GetLoginQuery : IRequest<Response<bool>>
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
    }

    public class GetLoginQueryHandler : IRequestHandler<GetLoginQuery, Response<bool>>
    {
        private readonly ILoginRepositoryAsync _loginRepository;

        public GetLoginQueryHandler(ILoginRepositoryAsync loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<Response<bool>> Handle(GetLoginQuery query, CancellationToken cancellationToken)
        {
            var existsHQ = await _loginRepository.IsExistsUsuarioAsync(query.Usuario, query.Senha);
            return new Response<bool>(existsHQ);
        }
	}
}
