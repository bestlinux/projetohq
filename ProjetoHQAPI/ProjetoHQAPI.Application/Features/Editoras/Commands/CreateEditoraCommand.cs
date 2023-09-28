using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Editoras.Commands
{
    public partial class CreateEditoraCommand : IRequest<Response<Guid>>
    {
        public string Nome { get; set; }
    }

    public class CreateEditoraCommandHandler : IRequestHandler<CreateEditoraCommand, Response<Guid>>
    {
        private readonly IEditoraRepositoryAsync _editoraRepository;
        private readonly IMapper _mapper;

        public CreateEditoraCommandHandler(IEditoraRepositoryAsync editoraRepository, IMapper mapper)
        {
            _editoraRepository = editoraRepository;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(CreateEditoraCommand request, CancellationToken cancellationToken)
        {
            var editora = _mapper.Map<Editora>(request);
            await _editoraRepository.AddAsync(editora);
            return new Response<Guid>(editora.Id);
        }
    }
}
