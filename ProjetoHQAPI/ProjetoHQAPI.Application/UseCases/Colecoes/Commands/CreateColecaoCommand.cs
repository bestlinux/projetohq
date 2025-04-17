using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Colecoes.Commands
{
    public partial class CreateColecaoCommand : IRequest<Guid>
    {
        public Guid? ColecaoId { get; set; }
        public string Descricao { get; set; }

        public string Arquivo { get; set; }
    }

    public class CreateColecaoCommandHandler(IUnitOfWork unitOfWork,
    IColecaoRepository colecaoRepository,
    IMapper mapper,
    IMediator mediator) : IRequestHandler<CreateColecaoCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IColecaoRepository _colecaoRepository = colecaoRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;

        public async Task<Guid> Handle(CreateColecaoCommand request, CancellationToken cancellationToken)
        {
            var colecao = _mapper.Map<Colecao>(request);
            await _colecaoRepository.AddAsync(colecao);
            await _unitOfWork.Commit(cancellationToken);

            await _mediator.Publish(new ColecaoActionNotification
            {
                ColecaoId = request.ColecaoId,
                Action = ActionNotification.Created
            }, cancellationToken);

            return colecao.Id;
        }
    }
}
