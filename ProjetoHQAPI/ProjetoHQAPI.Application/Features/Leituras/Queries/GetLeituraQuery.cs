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
    public class GetLeituraQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string Titulo { get; set; }
        public string Editora { get; set; }
    }

    public class GetLeituraQueryHandler : IRequestHandler<GetLeituraQuery, PagedResponse<IEnumerable<Entity>>>
    {
        private readonly ILeituraRepositoryAsync _leituraRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public GetLeituraQueryHandler(ILeituraRepositoryAsync leituraRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _leituraRepository = leituraRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetLeituraQuery request, CancellationToken cancellationToken)
        {
            var validFilter = request;
            var pagination = request;
            //filtered fields security
            if (!string.IsNullOrEmpty(validFilter.Fields))
            {
                //limit to fields in view model
                validFilter.Fields = _modelHelper.ValidateModelFields<GetLeituraViewModel>(validFilter.Fields);
            }
            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetLeituraViewModel>();
            }
            // query based on filter
            var entityHQs = await _leituraRepository.GetPagedLeituraReponseAsync(validFilter);
            var data = entityHQs.data;
            RecordsCount recordCount = entityHQs.recordsCount;
            // response wrapper
            return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, validFilter.PageSize, recordCount);
        }
    }
}
