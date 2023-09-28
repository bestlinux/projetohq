using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Application.Features.Colecoes.Queries;
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
    public class ColecaoRepositoryAsync : GenericRepositoryAsync<Colecao>, IColecaoRepositoryAsync
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<Colecao> _colecao;
        private IDataShapeHelper<Colecao> _dataShaper;
        private readonly IMockService _mockData;

        public ColecaoRepositoryAsync(ApplicationDbContext dbContext,
           IDataShapeHelper<Colecao> dataShaper, IMockService mockData) : base(dbContext)
        {
            _dbContext = dbContext;
            _colecao = dbContext.Set<Colecao>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }


        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedColecaoReponseAsync(GetColecaoQuery requestParameter)
        {
            var descricao = requestParameter.Descricao;
      
            var pageNumber = requestParameter.PageNumber;
            var pageSize = requestParameter.PageSize;
            var orderBy = requestParameter.OrderBy;
            var fields = requestParameter.Fields;

            int recordsTotal, recordsFiltered;

            // Setup IQueryable
            var result = _colecao
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
                result = result.Select<Colecao>("new(" + fields + ")");
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
