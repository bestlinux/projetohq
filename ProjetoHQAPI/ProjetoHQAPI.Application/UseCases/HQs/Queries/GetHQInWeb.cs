using AutoMapper;
using MediatR;
using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.HQs.Queries
{
    public class GetHQInWeb : IRequest<IReadOnlyCollection<GetHQResponse>>
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
    public class GetHQInWebQueryHandler(IHQRepository hqRepository, IMapper mapper) : IRequestHandler<GetHQInWeb, IReadOnlyCollection<GetHQResponse>>
    {
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetHQResponse>> Handle(GetHQInWeb request, CancellationToken cancellationToken)
        {
            var leitura = await _hqRepository.GetPagedHQInWebReponseAsync(request.Titulo, request.Editora, request.Categoria, request.Genero, request.Status, request.Formato, request.NumeroEdicao);

            return leitura.Select(_mapper.Map<GetHQResponse>).ToList();
        }
    }
}
