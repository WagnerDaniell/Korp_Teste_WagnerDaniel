using FluentValidation;
using Korp.Estoque.Application.DTOs;

namespace Korp.Estoque.Application.Validators
{
    public class UpdateProdutoValidator : AbstractValidator<UpdateDescricaoRequest>
    {
        public UpdateProdutoValidator()
        {
            RuleFor(x => x.NovaDescricao)
                .NotEmpty().WithMessage("A descrição (nome) do produto é obrigatória.")
                .MinimumLength(3).WithMessage("A descrição deve ter pelo menos 3 caracteres.")
                .MaximumLength(200).WithMessage("A descrição deve ter no máximo 200 caracteres.");
        }
    }
}