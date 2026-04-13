using Korp.Faturamento.Application.DTOs.Response;
using Korp.Faturamento.Domain.Exceptions;
using Korp.Faturamento.Domain.Repositories;

namespace Korp.Faturamento.Application.UseCases.NotasFiscais;

public class ReadNotaUseCase
{
    private readonly INotaFiscalRepository _notaRepository;

    public ReadNotaUseCase(INotaFiscalRepository notaRepository)
    {
        _notaRepository = notaRepository;
    }

    public async Task<List<NotaFiscalResponse>> ExecuteGetAll()
    {
        var notas = await _notaRepository.GetAllAsync();

        return notas.Select(nota => new NotaFiscalResponse(
            nota.Id,
            nota.Numero,
            nota.Status.ToString(),
            nota.CreatedAt,
            nota.Itens.Select(i => new ItemNotaResponse(
                i.Id,
                i.CodigoProduto,
                i.Quantidade
            )).ToList()
        )).ToList();
    }

    public async Task<NotaFiscalResponse> ExecuteGetById(Guid id)
    {
        var nota = await _notaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Nota fiscal {id} não encontrada.");

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