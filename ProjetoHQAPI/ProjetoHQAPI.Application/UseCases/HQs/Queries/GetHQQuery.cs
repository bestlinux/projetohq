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

        public int? Lido { get; set; }

        public string? Search { get; set; }

        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public string? OrderBy { get; set; }

        public bool? OrderDesc { get; set; }
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
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var term = request.Search.Trim();
                    hqs = hqs.Where(h =>
                        (h.Titulo != null && h.Titulo.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (h.Editora != null && h.Editora.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (h.Personagens != null && h.Personagens.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (h.Licenciador != null && h.Licenciador.Contains(term, StringComparison.OrdinalIgnoreCase))
                    );
                }

                if (!string.IsNullOrWhiteSpace(request.Editora))
                {
                    hqs = hqs.Where(h => h.Editora != null && h.Editora.Contains(request.Editora, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(request.Titulo))
                {
                    hqs = hqs.Where(h => h.Titulo != null && h.Titulo.Contains(request.Titulo, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(request.AnoLancamento))
                {
                    hqs = hqs.Where(h => h.DataPublicacao != null && h.DataPublicacao.Contains(request.AnoLancamento, StringComparison.OrdinalIgnoreCase));
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
                if (request.Lido.HasValue)
                {
                    hqs = hqs.Where(h => h.Lido == request.Lido.Value);
                }

                // Sorting
                if (!string.IsNullOrWhiteSpace(request.OrderBy))
                {
                    var isDesc = request.OrderDesc ?? false;
                    hqs = request.OrderBy.ToLowerInvariant() switch
                    {
                        "titulo" => isDesc ? hqs.OrderByDescending(h => h.Titulo) : hqs.OrderBy(h => h.Titulo),
                        "editora" => isDesc ? hqs.OrderByDescending(h => h.Editora) : hqs.OrderBy(h => h.Editora),
                        "ano" or "datapublicacao" => isDesc ? hqs.OrderByDescending(h => h.DataPublicacao) : hqs.OrderBy(h => h.DataPublicacao),
                        "edicao" or "numeroedicao" => isDesc ? hqs.OrderByDescending(h => h.NumeroEdicao) : hqs.OrderBy(h => h.NumeroEdicao),
                        "lido" => isDesc ? hqs.OrderByDescending(h => h.Lido) : hqs.OrderBy(h => h.Lido),
                        _ => hqs
                    };
                }

                // Pagination (only if requested)
                if (request.Page.HasValue && request.Page.Value > 0 && request.PageSize.HasValue && request.PageSize.Value > 0)
                {
                    hqs = hqs.Skip((request.Page.Value - 1) * request.PageSize.Value).Take(request.PageSize.Value);
                }
            }

            return hqs.Select(_mapper.Map<GetHQResponse>).ToList();
        }
    }
}
