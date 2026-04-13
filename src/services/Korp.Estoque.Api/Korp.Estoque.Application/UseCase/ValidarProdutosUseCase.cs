using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class ValidarProdutosUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public ValidarProdutosUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task Execute(ValidarProdutosRequest request)
    {
        var codigosDistintos = request.Codigos.Distinct().ToList();

        var encontrados = await _produtoRepository
            .GetCodigosExistentesAsync(codigosDistintos);

        var naoEncontrados = codigosDistintos
            .Except(encontrados)
            .ToList();

        if (naoEncontrados.Count != 0)
            throw new BusinessException(
                $"Produtos não encontrados no estoque: {string.Join(", ", naoEncontrados)}");
    }
}