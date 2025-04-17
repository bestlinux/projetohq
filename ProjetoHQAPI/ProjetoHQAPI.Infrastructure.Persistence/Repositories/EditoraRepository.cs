using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{

    public class EditoraRepository(ApplicationDbContext context) : Repository<Editora>(context), IEditoraRepository
    {
        public async Task<bool> IsUniqueEditoraAsync(string nome)
        {
            return await _db.Editoras
                .AllAsync(p => p.Nome != nome);
        }
    }
}
