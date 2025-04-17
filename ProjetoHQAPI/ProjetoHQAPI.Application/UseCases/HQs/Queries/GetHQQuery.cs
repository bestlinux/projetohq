using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.Leituras.Queries;
using ProjetoHQApi.Application.UseCases.HQs.Queries;

namespace ProjetoHQApi.Application.UseCases.HQs.Queries
{
    public class GetHQQuery : IRequest<IReadOnlyCollection<GetHQResponse>>
    {
        public string Editora { get; set; }

        public string Titulo { get; set; }

        public string AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

    }

    public class GetAllHQsQueryHandler(IHQRepository hqRepository, IMapper mapper) : IRequestHandler<GetHQQuery, IReadOnlyCollection<GetHQResponse>>
    {
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetHQResponse>> Handle(GetHQQuery request, CancellationToken cancellationToken)
        {
            var leitura = await _hqRepository.GetAllAsync(cancellationToken);

            return leitura.Select(_mapper.Map<GetHQResponse>).ToList();
        }
    }
}
