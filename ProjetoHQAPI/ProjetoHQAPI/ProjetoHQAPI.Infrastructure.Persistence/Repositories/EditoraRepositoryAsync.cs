using ProjetoHQApi.Application.Features.Editoras.Queries;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
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

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    
    public class EditoraRepositoryAsync : GenericRepositoryAsync<Editora>, IEditoraRepositoryAsync
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<Editora> _editoras;
        private IDataShapeHelper<Editora> _dataShaper;
        private readonly IMockService _mockData;

        public EditoraRepositoryAsync(ApplicationDbContext dbContext,
            IDataShapeHelper<Editora> dataShaper, IMockService mockData) : base(dbContext)
        {
            _dbContext = dbContext;
            _editoras = dbContext.Set<Editora>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }

        public async Task<bool> IsUniqueEditoraAsync(string nome)
        {
            return await _editoras
                .AllAsync(p => p.Nome != nome);
        }

     
        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedEditoraReponseAsync(GetEditoraQuery requestParameter)
        {
            var nome = requestParameter.Nome;

            var pageNumber = requestParameter.PageNumber;
            var pageSize = requestParameter.PageSize;
            var orderBy = requestParameter.OrderBy;
            var fields = requestParameter.Fields;

            int recordsTotal, recordsFiltered;

            // Setup IQueryable
            var result = _editoras
                .AsNoTracking()
                .AsExpandable();

            // Count records total
            recordsTotal = await result.CountAsync();

            // filter data
            FilterByColumn(ref result, nome);

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
                result = result.Select<Editora>("new(" + fields + ")");
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

        private void FilterByColumn(ref IQueryable<Editora> editoras,
            string nome
            )
        {
            if (!editoras.Any())
                return;

            if (string.IsNullOrEmpty(nome))
                return;

            var predicate = PredicateBuilder.New<Editora>();

            if (!string.IsNullOrEmpty(nome))
                predicate = predicate.Or(p => p.Nome.Contains(nome.Trim()));

            editoras = editoras.Where(predicate);
        }
    }
}
