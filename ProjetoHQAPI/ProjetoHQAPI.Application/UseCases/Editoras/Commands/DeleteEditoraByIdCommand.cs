using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Editoras.Commands
{
    public class DeleteEditoraByIdCommand : IRequest<bool>
    {
        public Guid? EditoraId { get; set; }
    }

    public class DeleteEditoraByIdCommandHandler : IRequestHandler<DeleteEditoraByIdCommand, bool>
    {
        private readonly IEditoraRepository _editoraRepository;
        private readonly IMediator _mediator;

        public DeleteEditoraByIdCommandHandler(IEditoraRepository editoraRepository, IMediator mediator)
        {
            _editoraRepository = editoraRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteEditoraByIdCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var editora = await _editoraRepository.GetByIdAsync(command.EditoraId);

                if (editora == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada editora com id " + command.EditoraId,
                    }, cancellationToken);
                    return false;
                }
                await _editoraRepository.RemoveAsync(editora.Id);

                return true;
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao excluir a editora de id " + command.EditoraId,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }
        }
    }
}
