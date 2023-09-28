using ProjetoHQApi.Application.Exceptions;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Commands
{
    public class UpdateHQCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }
        public string Editora { get; set; }

        public string Titulo { get; set; }

        public string AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public string LinkDetalhes { get; set; }

        public string ThumbCapa { get; set; }

        public int Lido { get; set; }

        public class UpdateHQCommandHandler : IRequestHandler<UpdateHQCommand, Response<Guid>>
        {
            private readonly IHQRepositoryAsync _hqRepository;

            public UpdateHQCommandHandler(IHQRepositoryAsync hqRepository)
            {
                _hqRepository = hqRepository;
            }

            public async Task<Response<Guid>> Handle(UpdateHQCommand command, CancellationToken cancellationToken)
            {
                var hq = await _hqRepository.GetByIdAsync(command.Id);

                if (hq == null)
                {
                    throw new ApiException($"Titulo Not Found.");
                }
                else
                {

                    hq.NumeroEdicao = command.NumeroEdicao;
                    hq.Lido = command.Lido;

                    await _hqRepository.UpdateAsync(hq);
                    return new Response<Guid>(hq.Id);
                }
            }
        }
    }
}
