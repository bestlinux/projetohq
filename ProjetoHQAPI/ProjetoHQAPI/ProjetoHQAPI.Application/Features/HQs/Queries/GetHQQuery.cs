using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Queries
{ 
    public class GetHQQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string Titulo { get; set; }
        public string Editora { get; set; }
    }

    public class GetAllHQsQueryHandler : IRequestHandler<GetHQQuery, PagedResponse<IEnumerable<Entity>>>
    {
        private readonly IHQRepositoryAsync _hqRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public GetAllHQsQueryHandler(IHQRepositoryAsync hqRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _hqRepository = hqRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetHQQuery request, CancellationToken cancellationToken)
        {
            var validFilter = request;
            var pagination = request;
            //filtered fields security
            if (!string.IsNullOrEmpty(validFilter.Fields))
            {
                //limit to fields in view model
                validFilter.Fields = _modelHelper.ValidateModelFields<GetHQViewModel>(validFilter.Fields);
            }
            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetHQViewModel>();
            }
            // query based on filter
            var entityHQs = await _hqRepository.GetPagedHQReponseAsync(validFilter);
            var data = entityHQs.data;
            RecordsCount recordCount = entityHQs.recordsCount;
            // response wrapper
            return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, validFilter.PageSize, recordCount);
        }
    }
}
