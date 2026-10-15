using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Frases.Commands
{
    public class UpdateFraseCommand : IRequest<Guid?>
    {
        public Guid Id { get; set; }
        public string NomeHQ { get; set; }
        public string Autor { get; set; }
        public string Arquivo { get; set; }
    }

    public class UpdateFraseCommandHandler(IFraseRepository fraseRepository, IUnitOfWork unitOfWork, IMediator mediator) : IRequestHandler<UpdateFraseCommand, Guid?>
    {
        private readonly IFraseRepository _fraseRepository = fraseRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMediator _mediator = mediator;

        public async Task<Guid?> Handle(UpdateFraseCommand command, CancellationToken cancellationToken)
        {
            var frase = await _fraseRepository.GetByIdAsync(command.Id);

            if (frase == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada frase com id " + command.Id,
                }, cancellationToken);
                return null;
            }

            frase.NomeHQ = command.NomeHQ;
            frase.Autor = command.Autor;
            if (!string.IsNullOrWhiteSpace(command.Arquivo))
            {
                frase.Arquivo = command.Arquivo;
            }

            await _fraseRepository.UpdateAsync(frase);
            await _unitOfWork.Commit(cancellationToken);

            return frase.Id;
        }
    }
}
