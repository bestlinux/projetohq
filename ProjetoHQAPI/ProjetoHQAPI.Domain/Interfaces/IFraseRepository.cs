using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface IFraseRepository : IRepository<Frase>
    {
        Task<IEnumerable<Frase>> LocalizaTodasFrasesPorNomeHQAutor(string? nomeHq, string? autor, CancellationToken cancellationToken);
    }
}
