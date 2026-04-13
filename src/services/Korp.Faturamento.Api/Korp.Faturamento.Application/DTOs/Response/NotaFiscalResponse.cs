namespace Korp.Faturamento.Application.DTOs.Response;

public record NotaFiscalResponse(
    Guid Id,
    int Numero,
    string Status,
    DateTime CreatedAt,
    List<ItemNotaResponse> Itens);