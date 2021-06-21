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

namespace ProjetoHQApi.Application.Features.Frases.Queries
{
    public class GetFrasesQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string NomeHQ { get; set; }
        public string Autor { get; set; }
    }

    public class GetAllFrasesQueryHandler : IRequestHandler<GetFrasesQuery, PagedResponse<IEnumerable<Entity>>>
    {
        private readonly IFraseRepositoryAsync _frasesRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public GetAllFrasesQueryHandler(IFraseRepositoryAsync frasesRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _frasesRepository = frasesRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetFrasesQuery request, CancellationToken cancellationToken)
        {
            var validFilter = request;
            var pagination = request;
            //filtered fields security
            if (!string.IsNullOrEmpty(validFilter.Fields))
            {
                //limit to fields in view model
                validFilter.Fields = _modelHelper.ValidateModelFields<GetFrasesViewModel>(validFilter.Fields);
            }
            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetFrasesViewModel>();
            }
            // query based on filter
            var entityPositions = await _frasesRepository.GetPagedFrasesReponseAsync(validFilter);
            var data = entityPositions.data;
            RecordsCount recordCount = entityPositions.recordsCount;
            // response wrapper
            return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, validFilter.PageSize, recordCount);
        }
    }
}
