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
        public string? Editora { get; set; }

        public string? Titulo { get; set; }

        public string? AnoLancamento { get; set; }

        public int? NumeroEdicao { get; set; }

        public int? Categoria { get; set; }

        public int? Genero { get; set; }

        public int? Status { get; set; }

        public int? Formato { get; set; }

    }

    public class GetAllHQsQueryHandler(IHQRepository hqRepository, IMapper mapper) : IRequestHandler<GetHQQuery, IReadOnlyCollection<GetHQResponse>>
    {
        private readonly IHQRepository _hqRepository = hqRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyCollection<GetHQResponse>> Handle(GetHQQuery request, CancellationToken cancellationToken)
        {
            var hqs = await _hqRepository.GetAllAsync(cancellationToken);

            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Editora))
                {
                    hqs = hqs.Where(h => h.Editora != null && h.Editora.Contains(request.Editora, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(request.Titulo))
                {
                    hqs = hqs.Where(h => h.Titulo != null && h.Titulo.Contains(request.Titulo, StringComparison.OrdinalIgnoreCase));
                }
                if (request.NumeroEdicao.HasValue && request.NumeroEdicao.Value > 0)
                {
                    hqs = hqs.Where(h => h.NumeroEdicao == request.NumeroEdicao.Value);
                }
                if (request.Categoria.HasValue && request.Categoria.Value > 0)
                {
                    hqs = hqs.Where(h => h.Categoria == request.Categoria.Value);
                }
                if (request.Genero.HasValue && request.Genero.Value > 0)
                {
                    hqs = hqs.Where(h => h.Genero == request.Genero.Value);
                }
                if (request.Status.HasValue && request.Status.Value > 0)
                {
                    hqs = hqs.Where(h => h.Status == request.Status.Value);
                }
                if (request.Formato.HasValue && request.Formato.Value > 0)
                {
                    hqs = hqs.Where(h => h.Formato == request.Formato.Value);
                }
            }

            return hqs.Select(_mapper.Map<GetHQResponse>).ToList();
        }
    }
}
