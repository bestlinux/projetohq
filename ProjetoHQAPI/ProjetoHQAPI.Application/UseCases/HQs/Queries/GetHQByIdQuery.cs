using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using AutoMapper;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;

namespace ProjetoHQApi.Application.UseCases.HQs.Queries
{
    public class GetHQByIdQuery : IRequest<GetHQResponse>
    {
        public Guid Id { get; set; }
    }
    public class GetHQByIdQueryHandler(IHQRepository hqRepository, IMediator mediator, IMapper mapper) : IRequestHandler<GetHQByIdQuery, GetHQResponse>
    {
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;

        public async Task<GetHQResponse> Handle(GetHQByIdQuery query, CancellationToken cancellationToken)
        {
            var hq = await _hqRepository.GetByIdAsync(query.Id);

            if (hq == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada hq com id " + query.Id,
                }, cancellationToken);
                return null;
            }

            return _mapper.Map<GetHQResponse>(hq);
        }
    }
}
