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
            try
            {
                var hq = await _hqRepositoryAsync.GetByIdAsync(command.Id);

                if (hq == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada hq com id " + command.Id,
                    }, cancellationToken);
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(hq.Capa))
                {
                    string arquivo = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, hq.Capa);
                    if (File.Exists(arquivo))
                    {
                        File.Delete(arquivo);
                    }
                }

                await _hqRepositoryAsync.RemoveAsync(command.Id);
                return true;
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
        }
    }

}
