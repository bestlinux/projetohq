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

namespace ProjetoHQApi.Application.Features.Frases.Commands
{
    public class DeleteFraseByIdCommand : IRequest<Response<Guid>>
    {
        private static readonly string DIRETORIO_IMAGENS = @"D:\projetohqrepositorio\Frases\";

        public Guid Id { get; set; }

        public class DeleteFraseByIdCommandHandler : IRequestHandler<DeleteFraseByIdCommand, Response<Guid>>
        {
            private readonly IFraseRepositoryAsync _fraseRepository;

            public DeleteFraseByIdCommandHandler(IFraseRepositoryAsync fraseRepository)
            {
                _fraseRepository = fraseRepository;
            }

            public async Task<Response<Guid>> Handle(DeleteFraseByIdCommand command, CancellationToken cancellationToken)
            {
                var frase = await _fraseRepository.GetByIdAsync(command.Id);

                try
                {
                    if (frase == null) throw new ApiException($"Frase Not Found.");

                    string arquivo = Path.Combine(DIRETORIO_IMAGENS, frase.Arquivo);

                    File.Delete(arquivo);

                    await _fraseRepository.DeleteAsync(frase);
                }
                catch (Exception)
                {
                    throw;
                }
               
                return new Response<Guid>(frase.Id);
            }
        }
    }
}
