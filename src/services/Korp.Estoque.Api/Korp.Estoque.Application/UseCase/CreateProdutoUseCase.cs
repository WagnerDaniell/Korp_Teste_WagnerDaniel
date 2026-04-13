using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Application.Validators;
using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;
using UUIDNext;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class CreateProdutoUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public CreateProdutoUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<ProdutoResponse> Execute(ProdutoRequest request)
    {
        var validator = new ProdutoValidator();
        var resultValidation = await validator.ValidateAsync(request);

        if (!resultValidation.IsValid)
        {
            var errors = resultValidation.Errors.Select(e => e.ErrorMessage).ToList();
            throw new ValidationException(errors);
        }

        var isExisting = await _produtoRepository.GetByCodigoAsync(request.Codigo);
        if (isExisting != null)
            throw new ValidationException(new List<string> { "Produto já existe." });

        var produto = new Produto(
            Uuid.NewDatabaseFriendly(Database.PostgreSql),
            request.Codigo,
            request.Descricao,
            request.Saldo
        );

        await _produtoRepository.CreateAsync(produto);

        return new ProdutoResponse(produto.Id, produto.Codigo, produto.Descricao, produto.Saldo);
    }
}