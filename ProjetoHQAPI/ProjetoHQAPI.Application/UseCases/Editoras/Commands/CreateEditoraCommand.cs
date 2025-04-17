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
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Editoras.Commands
{
    public partial class CreateEditoraCommand : IRequest<Guid>
    {
        public Guid? EditoraId { get; set; }
        public string Nome { get; set; }
    }

    public class CreateEditoraCommandHandler(IEditoraRepository editoraRepository, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator) : IRequestHandler<CreateEditoraCommand, Guid>
    {
        private readonly IEditoraRepository _editoraRepository = editoraRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMediator _mediator = mediator;

        public async Task<Guid> Handle(CreateEditoraCommand request, CancellationToken cancellationToken)
        {
            var editora = _mapper.Map<Editora>(request);
            await _editoraRepository.AddAsync(editora);
            await _unitOfWork.Commit(cancellationToken);

            await _mediator.Publish(new EditoraActionNotification
            {
                EditoraId = request.EditoraId,
                Action = ActionNotification.Created
            }, cancellationToken);

            return editora.Id;
        }
    }
}
