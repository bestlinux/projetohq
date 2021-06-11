using ProjetoHQApi.Application.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Editoras.Commands
{
    public class CreateEditoraCommandValidator : AbstractValidator<CreateEditoraCommand>
    {
        private readonly IEditoraRepositoryAsync editoraRepository;

        public CreateEditoraCommandValidator(IEditoraRepositoryAsync editoraRepository)
        {
            this.editoraRepository = editoraRepository;

            RuleFor(p => p.Nome)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.")
                .MustAsync(IsUniqueEditoraAsync).WithMessage("{PropertyValue} já existe.");

        }

        private async Task<bool> IsUniqueEditoraAsync(string nome, CancellationToken cancellationToken)
        {
            return await editoraRepository.IsUniqueEditoraAsync(nome);
        }
    }
}
