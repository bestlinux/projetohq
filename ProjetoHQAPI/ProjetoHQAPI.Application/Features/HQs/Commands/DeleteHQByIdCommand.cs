using MediatR;
using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Commands
{
    public class DeleteHQByIdCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteHQByIdCommandHandler : IRequestHandler<DeleteHQByIdCommand, Response<Guid>>
        {
            private readonly IHQRepositoryAsync _hqRepositoryAsync;

            public DeleteHQByIdCommandHandler(IHQRepositoryAsync fraseRepository)
            {
                _hqRepositoryAsync = fraseRepository;
            }

            public async Task<Response<Guid>> Handle(DeleteHQByIdCommand command, CancellationToken cancellationToken)
            {
                var hq = await _hqRepositoryAsync.GetByIdAsync(command.Id);

                try
                {
                    if (hq == null) throw new ApiException($"Frase Not Found.");

                    string arquivo = Path.Combine(Constantes.Constantes.GetDIRETORIO_IMAGENS(), hq.Capa);

                    File.Delete(arquivo);

                    await _hqRepositoryAsync.DeleteAsync(hq);
                }
                catch (Exception)
                {
                    throw;
                }

                return new Response<Guid>(hq.Id);
            }
        }
    }
}
