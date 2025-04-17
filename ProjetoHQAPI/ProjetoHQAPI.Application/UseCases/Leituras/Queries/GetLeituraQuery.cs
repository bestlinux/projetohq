using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.UseCases.Frases.Queries;
using ProjetoHQApi.Application.UseCases.Leituras.Queries;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Leituras.Queries
{
    public class GetLeituraQuery : IRequest<IReadOnlyCollection<GetLeituraResponse>>
    {
        public string? Titulo { get; set; }
    }

    public class GetLeituraQueryHandler(ILeituraRepository leituraRepository, IMapper mapper) : IRequestHandler<GetLeituraQuery, IReadOnlyCollection<GetLeituraResponse>>
    {
        private readonly ILeituraRepository _leituraRepository = leituraRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetLeituraResponse>> Handle(GetLeituraQuery request, CancellationToken cancellationToken)
        {
            var leitura = await _leituraRepository.LocalizaTodasLeituras(request.Titulo);

            return leitura.Select(_mapper.Map<GetLeituraResponse>).ToList();
        }
    }
}
