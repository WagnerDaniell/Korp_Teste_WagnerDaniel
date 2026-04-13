using FluentValidation;
using Korp.Estoque.Application.DTOs;

namespace Korp.Estoque.Application.Validators
{
    public class ProdutoValidator : AbstractValidator<ProdutoRequest>
    {
        public ProdutoValidator()
        {
            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("O código do produto é obrigatório.")
                .MaximumLength(50).WithMessage("O código deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição (nome) do produto é obrigatória.")
                .MinimumLength(3).WithMessage("A descrição deve ter pelo menos 3 caracteres.")
                .MaximumLength(200).WithMessage("A descrição deve ter no máximo 200 caracteres.");

            RuleFor(x => x.Saldo)
                .NotNull().WithMessage("O saldo inicial é obrigatório.")
                .GreaterThanOrEqualTo(0).WithMessage("O saldo inicial não pode ser negativo."); 
        }
    }
}