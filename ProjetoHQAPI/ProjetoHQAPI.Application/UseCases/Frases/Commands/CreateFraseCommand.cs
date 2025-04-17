using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Frases.Commands
{
    public partial class CreateFraseCommand : IRequest<Guid>
    {
        public Guid FraseId { get; set; }
        public string NomeHQ { get; set; }
        public string Autor { get; set; }

        public string Arquivo { get; set; }
    }

    public class CreateFraseCommandHandler : IRequestHandler<CreateFraseCommand, Guid>
    {
        private readonly IFraseRepository _fraseRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateFraseCommandHandler(IFraseRepository fraseRepository, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _fraseRepository = fraseRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Guid> Handle(CreateFraseCommand request, CancellationToken cancellationToken)
        {
            var frase = _mapper.Map<Frase>(request);
            await _fraseRepository.AddAsync(frase);
            await _unitOfWork.Commit(cancellationToken);

            await _mediator.Publish(new ColecaoActionNotification
            {
                ColecaoId = request.FraseId,
                Action = ActionNotification.Created
            }, cancellationToken);

            return frase.Id;
        }
    }
}
