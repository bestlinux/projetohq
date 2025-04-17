using ProjetoHQApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;
using AutoMapper;
using ProjetoHQApi.Application.Services.Notifications;

namespace ProjetoHQApi.Application.UseCases.Editoras.Queries
{
    public class GetEditoraByIdQuery : IRequest<GetEditoraResponse>
    {
        public Guid Id { get; set; }
    }

    public class GetEditoraByIdQueryHandler(IEditoraRepository editoraRepository, IMediator mediator, IMapper mapper) : IRequestHandler<GetEditoraByIdQuery, GetEditoraResponse>
    {
        private readonly IEditoraRepository _editoraRepository = editoraRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;

        public async Task<GetEditoraResponse> Handle(GetEditoraByIdQuery query, CancellationToken cancellationToken)
        {
            var editora = await _editoraRepository.GetByIdAsync(query.Id);

            if (editora == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada editora com id " + query.Id,
                }, cancellationToken);
                return null;
            }

            return _mapper.Map<GetEditoraResponse>(editora);
        }
    }

}
