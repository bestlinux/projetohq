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
    public class DesejoRepository (ApplicationDbContext context) : Repository<Desejo>(context), IDesejoRepository
    {
       public async Task<bool> IsExistsTituloAndAnoInDesejoAsync(string hqTitulo, string ano)
        {
            var data2 = await _db.Desejos.Where(d => d.Titulo.Contains(hqTitulo, StringComparison.CurrentCultureIgnoreCase) && d.DataPublicacao.Contains(ano, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }      
    }
}
