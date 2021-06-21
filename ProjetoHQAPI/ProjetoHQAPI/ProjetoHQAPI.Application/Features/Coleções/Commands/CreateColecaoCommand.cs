using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Entities;

namespace ProjetoHQApi.Application.Features.Colecoes.Commands
{
    public partial class CreateColecaoCommand : IRequest<Response<Guid>>
    {
        public string Descricao { get; set; }

        public string Arquivo { get; set; }
    }

    public class CreateColecaoCommandHandler : IRequestHandler<CreateColecaoCommand, Response<Guid>>
    {
        private readonly IColecaoRepositoryAsync _colecaoRepository;
        private readonly IMapper _mapper;

        public CreateColecaoCommandHandler(IColecaoRepositoryAsync colecaoRepository, IMapper mapper)
        {
            _colecaoRepository = colecaoRepository;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(CreateColecaoCommand request, CancellationToken cancellationToken)
        {
            var colecao = _mapper.Map<Colecao>(request);
            await _colecaoRepository.AddAsync(colecao);
            return new Response<Guid>(colecao.Id);
        }
    }
}
