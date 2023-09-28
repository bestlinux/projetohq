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

namespace ProjetoHQApi.Application.Features.Desejos.Queries
{
    public class GetDesejoQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string Titulo { get; set; }
        public string Editora { get; set; }
    }

    public class GetAllDesejoQueryHandler : IRequestHandler<GetDesejoQuery, PagedResponse<IEnumerable<Entity>>>
    {
        private readonly IDesejoRepositoryAsync _desejoRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public GetAllDesejoQueryHandler(IDesejoRepositoryAsync desejoRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _desejoRepository = desejoRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetDesejoQuery request, CancellationToken cancellationToken)
        {
            var validFilter = request;
            var pagination = request;
            //filtered fields security
            if (!string.IsNullOrEmpty(validFilter.Fields))
            {
                //limit to fields in view model
                validFilter.Fields = _modelHelper.ValidateModelFields<GetDesejoViewModel>(validFilter.Fields);
            }
            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetDesejoViewModel>();
            }
            // query based on filter
            var desejo = await _desejoRepository.GetPagedDesejoReponseAsync(validFilter);
            var data = desejo.data;
            RecordsCount recordCount = desejo.recordsCount;
            // response wrapper
            return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, validFilter.PageSize, recordCount);
        }
    }
}
