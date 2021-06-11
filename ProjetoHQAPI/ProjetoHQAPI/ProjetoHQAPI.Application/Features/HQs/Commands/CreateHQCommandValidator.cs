using ProjetoHQApi.Application.Features.Positions.Commands.CreatePosition;
using ProjetoHQApi.Application.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Commands
{
    public class CreateHQCommandValidator : AbstractValidator<CreateHQCommand>
    {
        private readonly IHQRepositoryAsync hqRepository;

        public CreateHQCommandValidator(IHQRepositoryAsync hqRepository)
        {
            this.hqRepository = hqRepository;

            RuleFor(p => p.Titulo)
              .NotEmpty().WithMessage("{PropertyName} is required.")
              .NotNull()
              .MaximumLength(100).WithMessage("{PropertyName} must not exceed 50 characters.")
              .MustAsync(IsUniqueHQTituloAsync).WithMessage("{PropertyValue} já existe");

        }

        private async Task<bool> IsUniqueHQTituloAsync(string titulo, CancellationToken cancellationToken)
        {
            return await hqRepository.IsUniqueHQTituloAsync(titulo);
        }
    }
}
