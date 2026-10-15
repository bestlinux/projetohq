using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Editoras.Commands
{
    public class UpdateEditoraCommand : IRequest<Guid?>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
    }

    public class UpdateEditoraCommandHandler(IEditoraRepository editoraRepository, IUnitOfWork unitOfWork, IMediator mediator) : IRequestHandler<UpdateEditoraCommand, Guid?>
    {
        private readonly IEditoraRepository _editoraRepository = editoraRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMediator _mediator = mediator;

        public async Task<Guid?> Handle(UpdateEditoraCommand command, CancellationToken cancellationToken)
        {
            var editora = await _editoraRepository.GetByIdAsync(command.Id);

            if (editora == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada editora com id " + command.Id,
                }, cancellationToken);
                return null;
            }

            editora.Nome = command.Nome;
            await _editoraRepository.UpdateAsync(editora);
            await _unitOfWork.Commit(cancellationToken);

            await _mediator.Publish(new EditoraActionNotification
            {
                EditoraId = editora.Id,
                Action = ActionNotification.Updated
            }, cancellationToken);

            return editora.Id;
        }
    }
}
