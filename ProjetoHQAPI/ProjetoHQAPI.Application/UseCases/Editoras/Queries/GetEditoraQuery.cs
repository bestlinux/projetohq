using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;

namespace ProjetoHQApi.Application.UseCases.Editoras.Queries
{
    public class GetEditoraQuery : IRequest<IReadOnlyCollection<GetEditoraResponse>>
    {
        public string Nome { get; set; }
    }

    public class GetEditoraQueryQueryHandler(IEditoraRepository editoraRepository, IMapper mapper) : IRequestHandler<GetEditoraQuery, IReadOnlyCollection<GetEditoraResponse>>
    {
        private readonly IEditoraRepository _editoraRepository = editoraRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetEditoraResponse>> Handle(GetEditoraQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Editora>? editoras = null;

            if (request.Nome is not null)
                editoras = await _editoraRepository.LocalizaTodasEditorasPorNome(request.Nome, cancellationToken);
            else
                editoras = await _editoraRepository.GetAllAsync(cancellationToken);

            return editoras.Select(_mapper.Map<GetEditoraResponse>).ToList();
        }
    }
}
