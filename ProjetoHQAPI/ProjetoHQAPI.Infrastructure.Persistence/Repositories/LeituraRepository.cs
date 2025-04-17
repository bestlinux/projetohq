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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class LeituraRepository (ApplicationDbContext context) : Repository<Leitura>(context), ILeituraRepository
    {      
        public async Task<bool> IsExistsTituloAndAnoInLeituraAsync(string hqTitulo, string ano)
        {
            var data2 = await _db.Leituras.Where(d => d.Titulo.ToLower().Contains(hqTitulo.ToLower()) && d.DataPublicacao.ToLower().Contains(ano)).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<IEnumerable<Leitura>> LocalizaTodasLeituras(string titulo)
        {
            return await _db.Leituras.AsNoTracking()
                 .Where(b => EF.Functions.Like(b.Titulo, "%" + titulo + "%"))
                 .ToListAsync();
        }
    }
}
