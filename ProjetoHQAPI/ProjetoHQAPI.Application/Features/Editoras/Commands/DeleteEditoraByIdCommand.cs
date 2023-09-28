using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Editoras.Commands
{
    public class DeleteEditoraByIdCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteEditoraByIdCommandHandler : IRequestHandler<DeleteEditoraByIdCommand, Response<Guid>>
        {
            private readonly IEditoraRepositoryAsync _editoraRepository;

            public DeleteEditoraByIdCommandHandler(IEditoraRepositoryAsync editoraRepository)
            {
                _editoraRepository = editoraRepository;
            }

            public async Task<Response<Guid>> Handle(DeleteEditoraByIdCommand command, CancellationToken cancellationToken)
            {
                var position = await _editoraRepository.GetByIdAsync(command.Id);
                if (position == null) throw new ApiException($"Editora Not Found.");
                await _editoraRepository.DeleteAsync(position);
                return new Response<Guid>(position.Id);
            }
        }
    }
}
