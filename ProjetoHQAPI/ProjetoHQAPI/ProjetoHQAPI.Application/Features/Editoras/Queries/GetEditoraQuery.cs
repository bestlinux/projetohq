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

namespace ProjetoHQApi.Application.Features.Editoras.Queries
{
    public class GetEditoraQuery : QueryParameter, IRequest<PagedResponse<IEnumerable<Entity>>>
    {
        public string Nome { get; set; }
    }

    public class GetEditoraQueryQueryHandler : IRequestHandler<GetEditoraQuery, PagedResponse<IEnumerable<Entity>>>
    {
        private readonly IEditoraRepositoryAsync _editoraRepository;
        private readonly IMapper _mapper;
        private readonly IModelHelper _modelHelper;

        public GetEditoraQueryQueryHandler(IEditoraRepositoryAsync editoraRepository, IMapper mapper, IModelHelper modelHelper)
        {
            _editoraRepository = editoraRepository;
            _mapper = mapper;
            _modelHelper = modelHelper;
        }

        public async Task<PagedResponse<IEnumerable<Entity>>> Handle(GetEditoraQuery request, CancellationToken cancellationToken)
        {
            var validFilter = request;
            var pagination = request;

            //filtered fields security
            if (!string.IsNullOrEmpty(validFilter.Fields))
            {
                //limit to fields in view model
                validFilter.Fields = _modelHelper.ValidateModelFields<GetEditoraViewModel>(validFilter.Fields);
            }
            if (string.IsNullOrEmpty(validFilter.Fields))
            {
                //default fields from view model
                validFilter.Fields = _modelHelper.GetModelFields<GetEditoraViewModel>();
            }

            validFilter.OrderBy = "Nome";

            // query based on filter
            var entityEditora = await _editoraRepository.GetPagedEditoraReponseAsync(validFilter);
            var data = entityEditora.data;
            RecordsCount recordCount = entityEditora.recordsCount;

            // response wrapper
            return new PagedResponse<IEnumerable<Entity>>(data, validFilter.PageNumber, validFilter.PageSize, recordCount);
        }
    }
}
