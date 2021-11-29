using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Leituras.Queries
{
    public partial class PagedLeituraQuery : IRequest<PagedDataTableResponse<IEnumerable<Entity>>>
    {
        //strong type input parameters
        public int Draw { get; set; } //page number

        public int Start { get; set; } //Paging first record indicator. This is the start point in the current data set (0 index based - i.e. 0 is the first record).
        public int Length { get; set; } //page size
        public IList<Order> Order { get; set; } //Order by
        public Search Search { get; set; } //search criteria
        public IList<Column> Columns { get; set; } //select fields
    }

    public class PagedLeituraQueryHandler : IRequestHandler<PagedLeituraQuery, PagedDataTableResponse<IEnumerable<Entity>>>
    {
        private readonly ILeituraRepositoryAsync _leituraRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public PagedLeituraQueryHandler(ILeituraRepositoryAsync leituraRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _leituraRepository = leituraRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedDataTableResponse<IEnumerable<Entity>>> Handle(PagedLeituraQuery request, CancellationToken cancellationToken)
        {
            var validFilter = new GetLeituraQuery();

            // Draw map to PageNumber
            validFilter.PageNumber = (request.Start / request.Length) + 1;
            // Length map to PageSize
            validFilter.PageSize = request.Length;

            // Map order > OrderBy
            var colOrder = request.Order[0];
            switch (colOrder.Column)
            {
                //case 0:
                //    validFilter.OrderBy = colOrder.Dir == "asc" ? "Id" : "Id DESC";
                //    break;
                case 0:
                    validFilter.OrderBy = colOrder.Dir == "asc" ? "Lido" : "Lido DESC";
                    break;

            }

            // Map Search > searchable columns
            if (!string.IsNullOrEmpty(request.Search.Value))
            {
                //limit to fields in view model
                validFilter.Titulo = request.Search.Value;
                validFilter.Editora = request.Search.Value;
            }

            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetLeituraViewModel>();
            }
            // query based on filter
            var entityPositions = await _leituraRepository.GetPagedLeituraReponseAsync(validFilter);
            var data = entityPositions.data;
            RecordsCount recordCount = entityPositions.recordsCount;

            // response wrapper
            return new PagedDataTableResponse<IEnumerable<Entity>>(data, request.Draw, recordCount);
        }
    }
}
