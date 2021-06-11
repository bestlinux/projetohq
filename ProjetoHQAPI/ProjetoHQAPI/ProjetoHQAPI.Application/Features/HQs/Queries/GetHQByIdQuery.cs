using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Queries
{
    public class GetHQByIdQuery : IRequest<Response<HQ>>
    {
        public Guid Id { get; set; }

        public class GetHQByIdQueryHandler : IRequestHandler<GetHQByIdQuery, Response<HQ>>
        {
            private readonly IHQRepositoryAsync _hqRepository;

            public GetHQByIdQueryHandler(IHQRepositoryAsync hqRepository)
            {
                _hqRepository = hqRepository;
            }

            public async Task<Response<HQ>> Handle(GetHQByIdQuery query, CancellationToken cancellationToken)
            {
                var hq = await _hqRepository.GetByIdAsync(query.Id);
                if (hq == null) throw new ApiException($"HQ Not Found.");
                return new Response<HQ>(hq);
            }
        }
    }
}
