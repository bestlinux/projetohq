using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Application.Features.Frases.Queries;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
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
	public class FraseRepositoryAsync : GenericRepositoryAsync<Frase>, IFraseRepositoryAsync
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly DbSet<Frase> _frases;
		private IDataShapeHelper<Frase> _dataShaper;
		private readonly IMockService _mockData;

        public FraseRepositoryAsync(ApplicationDbContext dbContext,
           IDataShapeHelper<Frase> dataShaper, IMockService mockData) : base(dbContext)
        {
            _dbContext = dbContext;
            _frases = dbContext.Set<Frase>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }
              

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedFrasesReponseAsync(GetFrasesQuery requestParameter)
        {
            var nomeHQ = requestParameter.NomeHQ;
            var autor = requestParameter.Autor;

            var pageNumber = requestParameter.PageNumber;
            var pageSize = requestParameter.PageSize;
            var orderBy = requestParameter.OrderBy;
            var fields = requestParameter.Fields;

            int recordsTotal, recordsFiltered;

            // Setup IQueryable
            var result = _frases
                .AsNoTracking()
                .AsExpandable();

            // Count records total
            recordsTotal = await result.CountAsync();

            // filter data
            //FilterByColumn(ref result, nomeHQ, autor);

            // Count records after filter
            recordsFiltered = await result.CountAsync();

            //set Record counts
            var recordsCount = new RecordsCount
            {
                RecordsFiltered = recordsFiltered,
                RecordsTotal = recordsTotal
            };

            // set order by
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                result = result.OrderBy(orderBy);
            }

            // select columns
            if (!string.IsNullOrWhiteSpace(fields))
            {
                result = result.Select<Frase>("new(" + fields + ")");
            }
            // paging
            result = result
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // retrieve data to list
            var resultData = await result.ToListAsync();
            // shape data
            var shapeData = _dataShaper.ShapeData(resultData, fields);

            return (shapeData, recordsCount);
        }
    }   
}
