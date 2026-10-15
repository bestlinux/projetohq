using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class FraseRepository(ApplicationDbContext context) : Repository<Frase>(context), IFraseRepository
    {
        public async Task<IEnumerable<Frase>> LocalizaTodasFrasesPorNomeHQAutor(string nomeHq, string autor, CancellationToken cancellationToken)
        {
            return await _db.Frases.AsNoTracking()
              .Where(f => EF.Functions.Like(f.Autor, "%" + autor + "%")
                     && EF.Functions.Like(f.NomeHQ, "%" + nomeHq + "%"))
               .ToListAsync(cancellationToken);
        }
    }
}
