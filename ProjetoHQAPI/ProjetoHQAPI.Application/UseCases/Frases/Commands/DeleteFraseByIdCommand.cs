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

namespace ProjetoHQApi.Application.USeCase.Frases.Commands
{
    public class DeleteFraseByIdCommand : IRequest<bool>
    {
        public Guid FraseId { get; set; }
    }
    public class DeleteFraseByIdCommandHandler : IRequestHandler<DeleteFraseByIdCommand, bool>
    {
        private readonly IFraseRepository _fraseRepository;
        private readonly IMediator _mediator;

        public DeleteFraseByIdCommandHandler(IFraseRepository fraseRepository, IMediator mediator)
        {
            _fraseRepository = fraseRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteFraseByIdCommand command, CancellationToken cancellationToken)
        {
            var frase = await _fraseRepository.GetByIdAsync(command.FraseId);

            try
            {
                if (frase == null)
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Não foi encontrada frase com id " + command.FraseId,
                    }, cancellationToken);
                    return false;
                }

                string arquivo = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS_FRASES, frase.Arquivo);

                File.Delete(arquivo);

                await _fraseRepository.RemoveAsync(frase.Id);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao deletar a frase com id " + command.FraseId,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return false;
            }

            return true;
        }
    }
}
