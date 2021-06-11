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

namespace ProjetoHQApi.Application.Features.Editoras.Queries
{
    public class GetEditoraByIdQuery : IRequest<Response<Editora>>
    {
        public Guid Id { get; set; }

        public class GetEditoraByIdQueryHandler : IRequestHandler<GetEditoraByIdQuery, Response<Editora>>
        {
            private readonly IEditoraRepositoryAsync _editoraRepository;

            public GetEditoraByIdQueryHandler(IEditoraRepositoryAsync editoraRepository)
            {
                _editoraRepository = editoraRepository;
            }

            public async Task<Response<Editora>> Handle(GetEditoraByIdQuery query, CancellationToken cancellationToken)
            {
                var editora = await _editoraRepository.GetByIdAsync(query.Id);
                if (editora == null) throw new ApiException($"Editora Not Found.");
                return new Response<Editora>(editora);
            }
        }
    }
}
