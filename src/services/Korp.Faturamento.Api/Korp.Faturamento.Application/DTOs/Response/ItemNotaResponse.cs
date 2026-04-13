namespace Korp.Faturamento.Application.DTOs.Response;

public record ItemNotaResponse(
    Guid Id,
    string CodigoProduto,
    int Quantidade);