using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Colecoes.Queries
{
    public class GetColecaoQuery : IRequest<IReadOnlyCollection<GetColecaoResponse>>
    {
    }

    public class GetColecaoQueryHandler(IColecaoRepository colecaoRepository, IMapper mapper) : IRequestHandler<GetColecaoQuery, IReadOnlyCollection<GetColecaoResponse>>
    {
        private readonly IColecaoRepository _colecaoRepository = colecaoRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetColecaoResponse>> Handle(GetColecaoQuery request, CancellationToken cancellationToken)
        {
            var colecao = await _colecaoRepository.GetAllAsync(cancellationToken);

            return colecao.Select(_mapper.Map<GetColecaoResponse>).ToList();
        }
    }
}
