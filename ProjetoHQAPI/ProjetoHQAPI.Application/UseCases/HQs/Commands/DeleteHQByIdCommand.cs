using MediatR;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public class DeleteHQByIdCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
    public class DeleteHQByIdCommandHandler : IRequestHandler<DeleteHQByIdCommand, bool>
    {
        private readonly IHQRepository _hqRepositoryAsync;
        private readonly IMediator _mediator;

        public DeleteHQByIdCommandHandler(IHQRepository fraseRepository, IMediator mediator)
        {
            _hqRepositoryAsync = fraseRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteHQByIdCommand command, CancellationToken cancellationToken)
        {
            var hq = await _hqRepositoryAsync.GetByIdAsync(command.Id);

            try
            {
                if (hq == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada hq com id " + command.Id,
                    }, cancellationToken);
                    return false;
                }

                string arquivo = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, hq.Capa);

                File.Delete(arquivo);

                await _hqRepositoryAsync.RemoveAsync(command.Id);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao deletar a hq com id " + command.Id,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }

            return true;
        }
    }

}
