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

namespace ProjetoHQApi.Application.Features.Desejos.Commands
{
	public class DeleteDesejoByIdCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteHQByIdCommandHandler : IRequestHandler<DeleteDesejoByIdCommand, Response<Guid>>
        {
            private readonly IDesejoRepositoryAsync _desejoRepositoryAsync;

            public DeleteHQByIdCommandHandler(IDesejoRepositoryAsync desejoRepositoryAsync)
            {
                _desejoRepositoryAsync = desejoRepositoryAsync;
            }

            public async Task<Response<Guid>> Handle(DeleteDesejoByIdCommand command, CancellationToken cancellationToken)
            {
                var hq = await _desejoRepositoryAsync.GetByIdAsync(command.Id);

                try
                {
                    if (hq == null) throw new ApiException($"Desejo Not Found.");

                    string arquivo = Path.Combine(Constantes.Constantes.GetDIRETORIO_IMAGENS_DESEJO(), hq.Capa);

                    File.Delete(arquivo);

                    await _desejoRepositoryAsync.DeleteAsync(hq);
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
