using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class DeleteProdutoUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public DeleteProdutoUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task Execute(Guid id)
    {
        var produto = await _produtoRepository.GetByIdAsync(id);

        if (produto == null)
            throw new NotFoundException("Produto não encontrado.");

        await _produtoRepository.DeleteAsync(id);
    }
}