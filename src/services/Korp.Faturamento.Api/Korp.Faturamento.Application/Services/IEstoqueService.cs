using Korp.Faturamento.Application.DTOs.Request;

namespace Korp.Faturamento.Application.Services;

public interface IEstoqueService
{
    Task BaixarEstoqueAsync(List<BaixaEstoqueRequest> requests);
    Task ValidarProdutosAsync(List<string> codigos);
}