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

namespace ProjetoHQApi.Application.UseCases.Colecoes.Commands
{
    public partial class DeleteColecaoByIdCommand : IRequest<bool>
    {
        public Guid? ColecaoId { get; set; }
    }

    public class DeleteColecaoByIdCommandHandler(IMediator mediator, IColecaoRepository colecaoRepository) : IRequestHandler<DeleteColecaoByIdCommand, bool>
    {
        private readonly IColecaoRepository _colecaoRepository = colecaoRepository;
        private readonly IMediator _mediator = mediator;

        public async Task<bool> Handle(DeleteColecaoByIdCommand command, CancellationToken cancellationToken)
        {
            var colecao = await _colecaoRepository.GetByIdAsync(command.ColecaoId);

            try
            {
                if (colecao == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada colecao com id " + command.ColecaoId,
                    }, cancellationToken);
                    return false;
                }

                string arquivo = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_COLECAO, colecao.Arquivo);

                File.Delete(arquivo);

                await _colecaoRepository.RemoveAsync(command.ColecaoId);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao excluir a colecao de id " + command.ColecaoId,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }

            return true;
        }
    }
}
