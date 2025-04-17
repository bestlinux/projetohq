using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.Application.UseCases.HQs.Queries
{
    public class GetHQByEditoraQuery : IRequest<bool>
    {
        public string Editora { get; set; }
    }

    public class GetHQByEditoraQueryHandler(IHQRepository hqRepository) : IRequestHandler<GetHQByEditoraQuery, bool>
    {
        private readonly IHQRepository _hqRepository = hqRepository;

        public async Task<bool> Handle(GetHQByEditoraQuery query, CancellationToken cancellationToken)
        {
            var existsHQ = await _hqRepository.IsExistsEditoraInHQAsync(query.Editora);
            return existsHQ;
        }
    }
}
