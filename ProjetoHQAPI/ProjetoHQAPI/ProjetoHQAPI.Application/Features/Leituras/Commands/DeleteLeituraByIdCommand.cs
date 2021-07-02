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
	public class DeleteLeituraByIdCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteLeituraByIdCommandHandler : IRequestHandler<DeleteLeituraByIdCommand, Response<Guid>>
        {
            private readonly ILeituraRepositoryAsync _leituraRepositoryAsync;

            public DeleteLeituraByIdCommandHandler(ILeituraRepositoryAsync leituraRepositoryAsync)
            {
                _leituraRepositoryAsync = leituraRepositoryAsync;
            }

            public async Task<Response<Guid>> Handle(DeleteLeituraByIdCommand command, CancellationToken cancellationToken)
            {
                var hq = await _leituraRepositoryAsync.GetByIdAsync(command.Id);

                try
                {
                    if (hq == null) throw new ApiException($"Leitura Not Found.");

                    await _leituraRepositoryAsync.DeleteAsync(hq);
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
