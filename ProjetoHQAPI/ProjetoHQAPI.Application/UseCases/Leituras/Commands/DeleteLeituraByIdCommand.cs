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
    public class DeleteLeituraByIdCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteLeituraByIdCommandHandler(ILeituraRepository leituraRepositoryAsync, IMediator mediator) : IRequestHandler<DeleteLeituraByIdCommand, bool>
    {
        private readonly ILeituraRepository _leituraRepositoryAsync = leituraRepositoryAsync;
        private readonly IMediator _mediator = mediator;

        public async Task<bool> Handle(DeleteLeituraByIdCommand command, CancellationToken cancellationToken)
        {
            var leitura = await _leituraRepositoryAsync.GetByIdAsync(command.Id);

            try
            {
                if (leitura == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada leitura com id " + command.Id,
                    }, cancellationToken);
                    return false;
                }

                await _leituraRepositoryAsync.RemoveAsync(command.Id);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao deletar a leitura com id " + command.Id,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }

            return true;
        }
    }
}
