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

namespace ProjetoHQApi.Application.Features.HQs.Queries
{
    public class GetHQAdvancedSearchQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string Editora { get; set; }

        public string Titulo { get; set; }

        public string AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public int Lido { get; set; }

        public string Roteiro { get; set; }

        public string Personagens { get; set; }

        public class GetHQAdvancedSearchQueryHandler : IRequestHandler<GetHQAdvancedSearchQuery, PagedResponse<IEnumerable<Entity>>>
        {
            private readonly IHQRepositoryAsync _hqRepository;
            private readonly IMapper _mapper;
            private readonly IModelHelper _modelHelper;

            public GetHQAdvancedSearchQueryHandler(IHQRepositoryAsync hqRepository, IMapper mapper, IModelHelper modelHelper)
            {
                _hqRepository = hqRepository;
                _mapper = mapper;
                _modelHelper = modelHelper;
            }

            public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetHQAdvancedSearchQuery request, CancellationToken cancellationToken)
            {
                request.PageNumber = 1;
                request.PageSize = 50000;

                var validFilter = request;

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
                var entityHQs = await _hqRepository.GetPagedHQAdvancedSearchReponseAsync(validFilter);
                var data = entityHQs.data;
                RecordsCount recordCount = entityHQs.recordsCount;

                // response wrapper
                return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, request.PageSize, recordCount);
            }
        }
    }
}
