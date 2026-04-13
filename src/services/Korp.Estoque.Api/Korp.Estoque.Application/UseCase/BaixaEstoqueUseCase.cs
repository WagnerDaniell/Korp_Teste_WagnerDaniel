using Korp.Estoque.Application.DTOs;
using Korp.Estoque.Domain.Exceptions;
using Korp.Estoque.Domain.Repositories;

namespace Korp.Estoque.Application.UseCases.Produtos;

public class BaixaEstoqueUseCase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ITransactionExecutor _transactionExecutor;

    public BaixaEstoqueUseCase(
        IProdutoRepository produtoRepository,
        ITransactionExecutor transactionExecutor)
    {
        _produtoRepository = produtoRepository;
        _transactionExecutor = transactionExecutor;
    }

    public async Task Execute(List<BaixaEstoqueRequest> requests)
    {
        var requestsOrdenados = requests
            .OrderBy(x => x.Codigo)
            .ToList();

        var codigos = requestsOrdenados
            .Select(x => x.Codigo)
            .Distinct()
            .ToList();

        await _transactionExecutor.ExecuteAsync(async () =>
        {
            var produtos = await _produtoRepository.GetByCodigosForUpdateAsync(codigos);
            var produtosMap = produtos.ToDictionary(p => p.Codigo);

            var naoEncontrados = codigos
                .Where(c => !produtosMap.ContainsKey(c))
                .ToList();

            if (naoEncontrados.Count != 0)
                throw new NotFoundException(
                    $"Produtos não encontrados: {string.Join(", ", naoEncontrados)}");

            var saldoInsuficiente = requestsOrdenados
                .Where(item => produtosMap[item.Codigo].Saldo < item.Quantidade)
                .Select(item => $"{item.Codigo} (disponível: {produtosMap[item.Codigo].Saldo}, solicitado: {item.Quantidade})")
                .ToList();

            if (saldoInsuficiente.Count != 0)
                throw new ConflictException(
                    $"Saldo insuficiente para: {string.Join(" | ", saldoInsuficiente)}");

            foreach (var item in requestsOrdenados)
                produtosMap[item.Codigo].BaixarEstoque(item.Quantidade);
        });
    }
}