using MediatR;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Desejos.Commands
{
    public partial class DeleteDesejoByIdCommand : IRequest<bool>
    {
        public Guid? DesejoId { get; set; }
    }
    public class DeleteHQByIdCommandHandler(IDesejoRepository desejoRepositoryAsync, IMediator mediator) : IRequestHandler<DeleteDesejoByIdCommand, bool>
    {
        private readonly IDesejoRepository _desejoRepositoryAsync = desejoRepositoryAsync;
        private readonly IMediator _mediator = mediator;

        public async Task<bool> Handle(DeleteDesejoByIdCommand command, CancellationToken cancellationToken)
        {
            var desejo = await _desejoRepositoryAsync.GetByIdAsync(command.DesejoId);

            try
            {
                if (desejo == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada desejo com id " + command.DesejoId,
                    }, cancellationToken);
                    return false;
                }

                string arquivo = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_DESEJOS, desejo.Capa);

                File.Delete(arquivo);

                await _desejoRepositoryAsync.RemoveAsync(desejo.Id);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao excluir o desejo de id " + command.DesejoId,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }

            return true;
        }
    }
}
