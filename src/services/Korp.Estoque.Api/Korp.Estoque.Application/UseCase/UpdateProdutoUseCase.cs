using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Application.Validators;
using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class UpdateProdutoUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public UpdateProdutoUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<ProdutoResponse> Execute(Guid id, UpdateDescricaoRequest request)
    {
        var validator = new UpdateProdutoValidator();
        var resultValidation = await validator.ValidateAsync(request);

        if (!resultValidation.IsValid)
        {
            var errors = resultValidation.Errors.Select(e => e.ErrorMessage).ToList();
            throw new ValidationException(errors);
        }

        var produto = await _produtoRepository.GetByIdAsync(id);

        if (produto == null)
            throw new NotFoundException("Produto não encontrado.");

        produto.Atualizar(request.NovaDescricao);

        await _produtoRepository.UpdateAsync(produto);

        return new ProdutoResponse(
            produto.Id,
            produto.Codigo,
            produto.Descricao,
            produto.Saldo
        );
    }
}