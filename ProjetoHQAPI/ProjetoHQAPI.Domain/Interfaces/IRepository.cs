﻿using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Entities;
using System.Linq.Expressions;

namespace ProjetoHQApi.Domain.Interfaces;

public interface IRepository<T> : IDisposable where T : Entity
{
    Task AddAsync(T entity);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T> GetByIdAsync(Guid? id);
    Task UpdateAsync(T entity);
    Task RemoveAsync(Guid? id);

    //Esse método é projetado para realizar uma busca assíncrona
    //no contexto do EF Core, usando um predicado
    //(predicate) fornecido como argumento. O predicado será aplicado
    //à entidade T, que é o tipo genérico usado no método, para filtrar
    //os resultados da busca.
    Task<IEnumerable<T>>
    SearchAsync(Expression<Func<T, bool>> predicate);
}
