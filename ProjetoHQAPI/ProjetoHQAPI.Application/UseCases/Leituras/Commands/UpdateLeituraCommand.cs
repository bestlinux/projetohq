using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Leituras.Commands
{
    public class UpdateLeituraCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }

        public int Lido { get; set; }

        public DateTime DataLeitura { get; set; }
    }
    public class UpdateLeituraCommandHandler(ILeituraRepository leituraRepository, IUnitOfWork unitOfWork, IMediator mediator) : IRequestHandler<UpdateLeituraCommand, Nullable<Guid>>
    {
        private readonly ILeituraRepository _leituraRepository = leituraRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMediator _mediator = mediator;

        public async Task<Nullable<Guid>> Handle(UpdateLeituraCommand command, CancellationToken cancellationToken)
        {
            var leitura = await _leituraRepository.GetByIdAsync(command.Id);

            if (leitura == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada leitura com id " + command.Id,
                }, cancellationToken);
                return null;
            }
            else
            {
                leitura.Lido = command.Lido;

                if (command.Lido == 0)
                    leitura.DataLeitura = null;
                else
                    leitura.DataLeitura = command.DataLeitura;

                await _leituraRepository.UpdateAsync(leitura);
                await _unitOfWork.Commit(cancellationToken);

                return leitura.Id;
            }
        }
    }
}
