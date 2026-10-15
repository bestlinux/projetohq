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
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class DesejoRepository (ApplicationDbContext context) : Repository<Desejo>(context), IDesejoRepository
    {

        public async Task<bool> IsExistsTituloAndAnoInDesejo(string hqTitulo, string dtPublicacao)
        {
            var data2 = await _db.Desejos.AsNoTracking()
                .Where(b => b.Titulo == hqTitulo && b.DataPublicacao == dtPublicacao)
                .ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<IEnumerable<Desejo>> LocalizaTodosDesejosPorTitulo(string titulo, CancellationToken cancellationToken)
        {
            return await _db.Desejos.AsNoTracking()
                .Where(b => EF.Functions.Like(b.Titulo, "%" + titulo + "%"))
                .ToListAsync(cancellationToken);
        }
    }
}
