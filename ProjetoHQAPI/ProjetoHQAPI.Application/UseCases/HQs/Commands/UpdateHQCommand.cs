using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Application.Common.Converters;

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public class UpdateHQCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }
        public string? Editora { get; set; }

        public string? Titulo { get; set; }

        public string? AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public string? LinkDetalhes { get; set; }

        public string? ThumbCapa { get; set; }

        public string? Preco { get; set; }

        public string? Licenciador { get; set; }

        public string? Personagens { get; set; }

        public string? DesenhosRoteirosArteFinalCores { get; set; }

        public int NumeroPaginas { get; set; }

        [JsonConverter(typeof(BoolOrIntJsonConverter))]
        public int Lido { get; set; }
    }

    public class UpdateHQCommandHandler : IRequestHandler<UpdateHQCommand, Nullable<Guid>>
    {
        private readonly IHQRepository _hqRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public UpdateHQCommandHandler(IHQRepository hqRepository, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _hqRepository = hqRepository;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Nullable<Guid>> Handle(UpdateHQCommand command, CancellationToken cancellationToken)
        {
            var hq = await _hqRepository.GetByIdAsync(command.Id);

            if (hq == null)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Não foi encontrada hq com id " + command.Id,
                }, cancellationToken);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(command.Titulo)) hq.Titulo = command.Titulo;
            if (!string.IsNullOrWhiteSpace(command.Editora)) hq.Editora = command.Editora;
            if (!string.IsNullOrWhiteSpace(command.AnoLancamento)) hq.DataPublicacao = command.AnoLancamento;
            if (command.NumeroEdicao > 0) hq.NumeroEdicao = command.NumeroEdicao;
            if (command.Categoria > 0) hq.Categoria = command.Categoria;
            if (command.Genero > 0) hq.Genero = command.Genero;
            if (command.Status > 0) hq.Status = command.Status;
            if (command.Formato > 0) hq.Formato = command.Formato;
            if (!string.IsNullOrWhiteSpace(command.ThumbCapa)) hq.ThumbCapa = command.ThumbCapa;
            if (!string.IsNullOrWhiteSpace(command.LinkDetalhes)) hq.LinkDetalhes = command.LinkDetalhes;
            if (!string.IsNullOrWhiteSpace(command.Preco)) hq.Preco = command.Preco;
            if (!string.IsNullOrWhiteSpace(command.Licenciador)) hq.Licenciador = command.Licenciador;
            if (!string.IsNullOrWhiteSpace(command.Personagens)) hq.Personagens = command.Personagens;
            if (!string.IsNullOrWhiteSpace(command.DesenhosRoteirosArteFinalCores)) hq.DesenhosRoteirosArteFinalCores = command.DesenhosRoteirosArteFinalCores;
            if (command.NumeroPaginas > 0) hq.NumeroPaginas = command.NumeroPaginas;
            hq.Lido = command.Lido;

            await _hqRepository.UpdateAsync(hq);
            await _unitOfWork.Commit(cancellationToken);

            return hq.Id;
        }
    }
}
