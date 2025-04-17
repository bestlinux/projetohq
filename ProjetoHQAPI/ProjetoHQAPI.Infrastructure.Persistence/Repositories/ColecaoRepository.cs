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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class ColecaoRepository(ApplicationDbContext context) : Repository<Colecao>(context), IColecaoRepository
    {
    }
}
