using Korp.Faturamento.Application.DTOs.Request;
using Korp.Faturamento.Application.DTOs.Response;
using Korp.Faturamento.Application.Services;
using Korp.Faturamento.Application.Validators;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Exceptions;
using Korp.Faturamento.Domain.Repositories;
using UUIDNext;

namespace Korp.Faturamento.Application.UseCases.NotasFiscais;

public class CreateNotaUseCase
{
    private readonly INotaFiscalRepository _notaRepository;
    private readonly IEstoqueService _estoqueService;

    public CreateNotaUseCase(
        INotaFiscalRepository notaRepository,
        IEstoqueService estoqueService)
    {
        _notaRepository = notaRepository;
        _estoqueService = estoqueService;
    }

    public async Task<NotaFiscalResponse> Execute(CreateNotaRequest request)
    {
        var validator = new CreateNotaValidator();
        var resultValidation = await validator.ValidateAsync(request);

        if (!resultValidation.IsValid)
        {
            var errors = resultValidation.Errors.Select(e => e.ErrorMessage).ToList();
            throw new ValidationException(errors);
        }

        var codigos = request.Itens
            .Select(i => i.CodigoProduto)
            .Distinct()
            .ToList();

        await _estoqueService.ValidarProdutosAsync(codigos);

        var itens = request.Itens.Select(i => new ItemNotaFiscal(
            Uuid.NewDatabaseFriendly(Database.PostgreSql),
            i.CodigoProduto,
            i.Quantidade
        )).ToList();

        var nota = new NotaFiscal(
            Uuid.NewDatabaseFriendly(Database.PostgreSql),
            itens
        );

        await _notaRepository.CreateAsync(nota);

        return new NotaFiscalResponse(
            nota.Id,
            nota.Numero,
            nota.Status.ToString(),
            nota.CreatedAt,
            nota.Itens.Select(i => new ItemNotaResponse(
                i.Id,
                i.CodigoProduto,
                i.Quantidade
            )).ToList()
        );
    }
}