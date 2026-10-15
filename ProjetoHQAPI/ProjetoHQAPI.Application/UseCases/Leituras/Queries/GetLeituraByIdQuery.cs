using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using AutoMapper;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Leituras.Queries
{
    public class GetLeituraByIdQuery : IRequest<GetLeituraResponse>
    {
        public Guid Id { get; set; }
    }

    public class GetLeituraByIdQueryHandler(ILeituraRepository leituraRepository, IMediator mediator, IMapper mapper) : IRequestHandler<GetLeituraByIdQuery, GetLeituraResponse>
    {
        private readonly ILeituraRepository _leituraRepository = leituraRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;

        public async Task<GetLeituraResponse> Handle(GetLeituraByIdQuery query, CancellationToken cancellationToken)
        {
            var leitura = await _leituraRepository.GetByIdAsync(query.Id);

            if (leitura == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada leitura com id " + query.Id,
                }, cancellationToken);
                return null;
            }

            return _mapper.Map<GetLeituraResponse>(leitura);
        }
    }
}
