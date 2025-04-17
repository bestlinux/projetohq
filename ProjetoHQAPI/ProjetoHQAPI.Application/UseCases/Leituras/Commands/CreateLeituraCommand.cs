using AutoMapper;
using MediatR;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProjetoHQApi.Application.UseCases.Leituras.Commands
{
    public partial class CreateLeituraCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }

        public string Capa { get; set; }

        public int Lido { get; set; }

        public string DataPublicacao { get; set; }
    }

    public class CreateLeituraCommandHandler(ILeituraRepository leituraRepository, IMapper mapper, IMediator mediator, IUnitOfWork unitOfWork) : IRequestHandler<CreateLeituraCommand, Nullable<Guid>>
    {
        private readonly ILeituraRepository _leituraRepository = leituraRepository;
        private readonly IMediator _mediator = mediator;
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Nullable<Guid>> Handle(CreateLeituraCommand request, CancellationToken cancellationToken)
        {
            if (await _leituraRepository.IsExistsTituloAndAnoInLeituraAsync(request.Titulo, request.DataPublicacao))
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Já existe o titulo " + request.Titulo + " com a data de publicação " + request.DataPublicacao + " na lista de leitura",
                }, cancellationToken);
                return null;
            }

            request.Lido = 0;           
            var leitura = _mapper.Map<Leitura>(request);

            try
            {
                await _leituraRepository.AddAsync(leitura);
                await _unitOfWork.Commit(cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao criar a leitura com id " + request.Id,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return null;
            }

            return leitura.Id;
        }
    }
}
