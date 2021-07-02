using LinqKit;
using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Application.Features.Leituras.Queries;
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
    public class LeituraRepositoryAsync : GenericRepositoryAsync<Leitura>, ILeituraRepositoryAsync
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<Leitura> _leituras;
        private IDataShapeHelper<Leitura> _dataShaper;
        private readonly IMockService _mockData;

        public LeituraRepositoryAsync(ApplicationDbContext dbContext,
            IDataShapeHelper<Leitura> dataShaper, IMockService mockData) : base(dbContext)
        {
            _dbContext = dbContext;
            _leituras = dbContext.Set<Leitura>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }

        public async Task<bool> IsExistsTituloAndAnoInLeituraAsync(string hqTitulo, string ano)
        {
            var data2 = await _leituras.Where(d => d.Titulo.ToLower().Contains(hqTitulo.ToLower()) && d.DataPublicacao.ToLower().Contains(ano)).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedLeituraReponseAsync(GetLeituraQuery requestParameter)
        {
            var hqTitulo = requestParameter.Titulo;
            var hqEditora = requestParameter.Editora;

            var pageNumber = requestParameter.PageNumber;
            var pageSize = requestParameter.PageSize;
            var orderBy = requestParameter.OrderBy;
            var fields = requestParameter.Fields;

            int recordsTotal, recordsFiltered;

            // Setup IQueryable
            var result = _leituras
                .AsNoTracking()
                .AsExpandable();

            // Count records total
            recordsTotal = await result.CountAsync();

            // filter data
            FilterByColumn(ref result, hqTitulo, hqEditora);

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
                result = result.Select<Leitura>("new(" + fields + ")");
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

        private void FilterByColumn(ref IQueryable<Leitura> leituras,
            string hqTitulo,
            string hqEditora
            )
        {
            if (!leituras.Any())
                return;

            if (string.IsNullOrEmpty(hqTitulo) &&
                string.IsNullOrEmpty(hqEditora))
                return;

            var predicate = PredicateBuilder.New<Leitura>();

            if (!string.IsNullOrEmpty(hqTitulo))
                predicate = predicate.Or(p => p.Titulo.Contains(hqTitulo.Trim()));

            leituras = leituras.Where(predicate);
        }
    }
}
