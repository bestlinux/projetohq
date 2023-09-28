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

namespace ProjetoHQApi.Application.Features.Leituras.Commands
{
    public partial class CreateLeituraCommand : IRequest<Response<Guid>>
    {
        public string Id { get; set; }
        public string Titulo { get; set; }

        public string Capa { get; set; }

        public int Lido { get; set; }

        public string DataPublicacao { get; set; }
    }

    public class CreateLeituraCommandHandler : IRequestHandler<CreateLeituraCommand, Response<Guid>>
    {
        private readonly ILeituraRepositoryAsync _leituraRepository;
        private readonly IMapper _mapper;

        public CreateLeituraCommandHandler(ILeituraRepositoryAsync leituraRepository, IMapper mapper)
        {
            _leituraRepository = leituraRepository;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(CreateLeituraCommand request, CancellationToken cancellationToken)
        {
            if (await _leituraRepository.IsExistsTituloAndAnoInLeituraAsync(request.Titulo, request.DataPublicacao))
            {
                Response<Guid> response = new()
                {
                    Message = "Já existe o titulo " + request.Titulo + " com a data de publicação " + request.DataPublicacao + " na lista de leitura"
                };
                return response;
            }

            request.Lido = 0;           
            var hq = _mapper.Map<Leitura>(request);

            try
            {
                await _leituraRepository.AddAsync(hq);
            }
            catch (Exception)
            {
                throw;
            }

            return new Response<Guid>(hq.Id);
        }
    }
}
