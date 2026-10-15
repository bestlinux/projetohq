using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Frases.Queries
{
    public class GetFraseByIdQuery : IRequest<GetFraseResponse>
    {
        public Guid Id { get; set; }
    }

    public class GetFraseByIdQueryHandler(IFraseRepository fraseRepository, IMapper mapper, IMediator mediator) : IRequestHandler<GetFraseByIdQuery, GetFraseResponse>
    {
        private readonly IFraseRepository _fraseRepository = fraseRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;

        public async Task<GetFraseResponse> Handle(GetFraseByIdQuery query, CancellationToken cancellationToken)
        {
            var frase = await _fraseRepository.GetByIdAsync(query.Id);

            if (frase == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada frase com id " + query.Id,
                }, cancellationToken);
                return null;
            }

            return _mapper.Map<GetFraseResponse>(frase);
        }
    }
}
