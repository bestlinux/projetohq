using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Queries
{
    public class GetHQByEditoraQuery : IRequest<Response<bool>>
    {
        public string Editora { get; set; }

        public class GetHQByEditoraQueryHandler : IRequestHandler<GetHQByEditoraQuery, Response<bool>>
        {
            private readonly IHQRepositoryAsync _hqRepository;

            public GetHQByEditoraQueryHandler(IHQRepositoryAsync hqRepository)
            {
                _hqRepository = hqRepository;
            }

            public async Task<Response<bool>> Handle(GetHQByEditoraQuery query, CancellationToken cancellationToken)
            {
                var existsHQ = await _hqRepository.IsExistsEditoraInHQAsync(query.Editora);
                return new Response<bool>(existsHQ);
            }
        }
    }
}
