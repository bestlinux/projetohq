using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Frases.Commands
{
    public partial class CreateFraseCommand : IRequest<Response<Guid>>
    {
        public string NomeHQ { get; set; }
        public string Autor { get; set; }

        public string Arquivo { get; set; }
    }

    public class CreateFraseCommandHandler : IRequestHandler<CreateFraseCommand, Response<Guid>>
    {
        private readonly IFraseRepositoryAsync _fraseRepository;
        private readonly IMapper _mapper;

        public CreateFraseCommandHandler(IFraseRepositoryAsync fraseRepository, IMapper mapper)
        {
            _fraseRepository = fraseRepository;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(CreateFraseCommand request, CancellationToken cancellationToken)
        {
            var frase = _mapper.Map<Frase>(request);
            await _fraseRepository.AddAsync(frase);
            return new Response<Guid>(frase.Id);
        }
    }
}
