using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class ReadProdutoUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public ReadProdutoUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<List<ProdutoResponse>> ExecuteGetAll()
    {
        var produtos = await _produtoRepository.GetAllAsync();

        return produtos.Select(p => new ProdutoResponse(
            p.Id,
            p.Codigo,
            p.Descricao,
            p.Saldo
        )).ToList();
    }

    public async Task<ProdutoResponse> ExecuteGetByCodigo(string codigo)
    {
        var produto = await _produtoRepository.GetByCodigoAsync(codigo);

        if (produto == null)
            throw new NotFoundException($"Produto {codigo} não encontrado.");

        return new ProdutoResponse(
            produto.Id,
            produto.Codigo,
            produto.Descricao,
            produto.Saldo
        );
    }
}