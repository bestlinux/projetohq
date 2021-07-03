using MediatR;
using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Leituras.Commands
{
    public class UpdateLeituraCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }   
        
        public int Lido { get; set; }

        public DateTime DataLeitura { get; set; }

        public class UpdateLeituraCommandHandler : IRequestHandler<UpdateLeituraCommand, Response<Guid>>
        {
            private readonly ILeituraRepositoryAsync _leituraRepository;

            public UpdateLeituraCommandHandler(ILeituraRepositoryAsync leituraRepository)
            {
                _leituraRepository = leituraRepository;
            }

            public async Task<Response<Guid>> Handle(UpdateLeituraCommand command, CancellationToken cancellationToken)
            {
                var hq = await _leituraRepository.GetByIdAsync(command.Id);

                if (hq == null)
                {
                    throw new ApiException($"Titulo Not Found.");
                }
                else
                {

                    hq.Lido = command.Lido;

                    if (command.Lido == 0)
                        hq.DataLeitura = null;
                    else
                        hq.DataLeitura = command.DataLeitura;

                    await _leituraRepository.UpdateAsync(hq);
                    return new Response<Guid>(hq.Id);
                }
            }
        }
    }
}
