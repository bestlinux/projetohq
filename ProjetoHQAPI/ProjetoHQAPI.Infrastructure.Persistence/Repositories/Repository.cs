using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Interfaces;
using System.Linq.Expressions;

namespace ProjetoHQApi.Infrastructure.Persistence.Repository
{

    public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<TEntity> DbSet;
        protected Repository(ApplicationDbContext db)
        {
            _db = db;
            DbSet = db.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await DbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(Guid? id)
        {
            var entity = await DbSet.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await DbSet.ToListAsync(cancellationToken);
            return entities;
        }

        public async Task AddAsync(TEntity entity)
        {
            DbSet.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            DbSet.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid? id)
        {
            var entity = await DbSet.FindAsync(id);
            DbSet.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}