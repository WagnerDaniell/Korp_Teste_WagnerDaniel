using FluentValidation;
using Korp.Faturamento.Application.DTOs.Request;

namespace Korp.Faturamento.Application.Validators;

public class CreateNotaValidator : AbstractValidator<CreateNotaRequest>
{
    public CreateNotaValidator()
    {
        RuleFor(x => x.Itens)
            .NotNull().WithMessage("A nota deve conter ao menos um item.")
            .NotEmpty().WithMessage("A nota deve conter ao menos um item.");

        RuleForEach(x => x.Itens).SetValidator(new ItemNotaValidator());
    }
}

public class ItemNotaValidator : AbstractValidator<ItemNotaRequest>
{
    public ItemNotaValidator()
    {
        RuleFor(x => x.CodigoProduto)
            .NotNull().WithMessage("Código do produto é obrigatório.")
            .NotEmpty().WithMessage("Código do produto é obrigatório.")
            .MaximumLength(50).WithMessage("Código do produto deve ter no máximo 50 caracteres.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
    }
}