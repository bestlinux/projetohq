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

namespace ProjetoHQApi.Application.Features.Colecoes.Commands
{
    public class DeleteColecaoByIdCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteColecaoByIdCommandHandler : IRequestHandler<DeleteColecaoByIdCommand, Response<Guid>>
        {
            private readonly IColecaoRepositoryAsync _colecaoRepository;

            public DeleteColecaoByIdCommandHandler(IColecaoRepositoryAsync colecaoRepository)
            {
                _colecaoRepository = colecaoRepository;
            }

            public async Task<Response<Guid>> Handle(DeleteColecaoByIdCommand command, CancellationToken cancellationToken)
            {
                var colecao = await _colecaoRepository.GetByIdAsync(command.Id);

                try
                {
                    if (colecao == null) throw new ApiException($"Colecao Not Found.");

                    string arquivo = Path.Combine(Constantes.Constantes.GetDIRETORIO_IMAGENS_COLECAO(), colecao.Arquivo);

                    File.Delete(arquivo);

                    await _colecaoRepository.DeleteAsync(colecao);
                }
                catch (Exception)
                {
                    throw;
                }

                return new Response<Guid>(colecao.Id);
            }
        }
    }
}
