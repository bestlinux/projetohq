using MediatR;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public class ToggleHQLidoCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public int? Lido { get; set; }
    }

    public class ToggleHQLidoCommandHandler(IHQRepository hqRepository, IUnitOfWork unitOfWork, IMediator mediator) : IRequestHandler<ToggleHQLidoCommand, bool>
    {
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMediator _mediator = mediator;

        public async Task<bool> Handle(ToggleHQLidoCommand request, CancellationToken cancellationToken)
        {
            var hq = await _hqRepository.GetByIdAsync(request.Id);
            if (hq == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "HQ não encontrada com id " + request.Id
                }, cancellationToken);
                return false;
            }

            if (request.Lido.HasValue)
            {
                hq.Lido = request.Lido.Value;
            }
            else
            {
                hq.Lido = hq.Lido == 1 ? 0 : 1;
            }

            await _hqRepository.UpdateAsync(hq);
            await _unitOfWork.Commit(cancellationToken);
            return true;
        }
    }
}
