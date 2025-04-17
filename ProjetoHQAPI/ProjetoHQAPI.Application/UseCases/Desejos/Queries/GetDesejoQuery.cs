using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.Colecoes.Queries;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Desejos.Queries
{
    public class GetDesejoQuery : IRequest<IReadOnlyCollection<GetDesejoResponse>>
    {
        public string Titulo { get; set; }
        public string Editora { get; set; }
    }

    public class GetAllDesejoQueryHandler(IDesejoRepository desejoRepository, IMapper mapper) : IRequestHandler<GetDesejoQuery, IReadOnlyCollection<GetDesejoResponse>>
    {
        private readonly IDesejoRepository _desejoRepository = desejoRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetDesejoResponse>> Handle(GetDesejoQuery request, CancellationToken cancellationToken)
        {
            var desejo = await _desejoRepository.GetAllAsync(cancellationToken);

            return desejo.Select(_mapper.Map<GetDesejoResponse>).ToList();
        }
    }
}
