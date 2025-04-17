using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public class UpdateHQCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }
        public string Editora { get; set; }

        public string Titulo { get; set; }

        public string AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public string LinkDetalhes { get; set; }

        public string ThumbCapa { get; set; }

        public int Lido { get; set; }
    }

    public class UpdateHQCommandHandler : IRequestHandler<UpdateHQCommand, Nullable<Guid>>
    {
        private readonly IHQRepository _hqRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public UpdateHQCommandHandler(IHQRepository hqRepository, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _hqRepository = hqRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Nullable<Guid>> Handle(UpdateHQCommand command, CancellationToken cancellationToken)
        {
            var hq = await _hqRepository.GetByIdAsync(command.Id);

            if (hq == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada hq com id " + command.Id,
                }, cancellationToken);
                return null;
            }
            else
            {

                hq.NumeroEdicao = command.NumeroEdicao;
                hq.Lido = command.Lido;

                await _hqRepository.UpdateAsync(hq);
                await _unitOfWork.Commit(cancellationToken);

                return hq.Id;
            }
        }
    }
}
