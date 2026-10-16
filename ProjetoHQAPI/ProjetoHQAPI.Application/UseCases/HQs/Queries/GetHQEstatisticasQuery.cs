using MediatR;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.HQs.Queries
{
    public class HQEstatisticasResponse
    {
        public int TotalHQs { get; set; }
        public int TotalLidas { get; set; }
        public int TotalNaoLidas { get; set; }
        public int TotalEditoras { get; set; }
        public int TotalPaginas { get; set; }
        public decimal ValorTotalEstimado { get; set; }
    }

    public class GetHQEstatisticasQuery : IRequest<HQEstatisticasResponse>
    {
    }

    public class GetHQEstatisticasQueryHandler(IHQRepository hqRepository) : IRequestHandler<GetHQEstatisticasQuery, HQEstatisticasResponse>
    {
        private readonly IHQRepository _hqRepository = hqRepository;

        public async Task<HQEstatisticasResponse> Handle(GetHQEstatisticasQuery request, CancellationToken cancellationToken)
        {
            var hqs = await _hqRepository.GetAllAsync(cancellationToken);
            var list = hqs.ToList();

            var totalHqs = list.Count;
            var totalLidas = list.Count(h => h.Lido == 1);
            var totalNaoLidas = totalHqs - totalLidas;
            var totalEditoras = list
                .Where(h => !string.IsNullOrWhiteSpace(h.Editora))
                .Select(h => h.Editora!.Trim().ToLower())
                .Distinct()
                .Count();
            var totalPaginas = list.Sum(h => h.NumeroPaginas);

            decimal valorTotal = 0;
            foreach (var h in list)
            {
                if (!string.IsNullOrWhiteSpace(h.Preco))
                {
                    var clean = h.Preco.Replace("R$", "", StringComparison.OrdinalIgnoreCase).Replace("$", "").Trim();
                    clean = clean.Replace(".", "").Replace(",", ".");
                    if (decimal.TryParse(clean, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
                    {
                        valorTotal += val;
                    }
                }
            }

            return new HQEstatisticasResponse
            {
                TotalHQs = totalHqs,
                TotalLidas = totalLidas,
                TotalNaoLidas = totalNaoLidas,
                TotalEditoras = totalEditoras,
                TotalPaginas = totalPaginas,
                ValorTotalEstimado = Math.Round(valorTotal, 2)
            };
        }
    }
}
