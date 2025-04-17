using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;
using ProjetoHQApi.Application.UseCases.Frases.Queries;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Frases.Queries
{
    public class GetFrasesQuery : IRequest<IReadOnlyCollection<GetFraseResponse>>
    {
        public string NomeHQ { get; set; }
        public string Autor { get; set; }
    }

    public class GetAllFrasesQueryHandler(IFraseRepository frasesRepository, IMapper mapper) : IRequestHandler<GetFrasesQuery, IReadOnlyCollection<GetFraseResponse>>
    {   
        private readonly IFraseRepository _frasesRepository = frasesRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetFraseResponse>> Handle(GetFrasesQuery request, CancellationToken cancellationToken)
        {
            var frase = await _frasesRepository.GetAllAsync(cancellationToken);

            return frase.Select(_mapper.Map<GetFraseResponse>).ToList();
        }
    }
}
