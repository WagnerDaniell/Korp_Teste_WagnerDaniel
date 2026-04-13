using Korp.Faturamento.Application.DTOs.Request;
using Korp.Faturamento.Application.DTOs.Response;
using Korp.Faturamento.Application.Services;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Exceptions;
using Korp.Faturamento.Domain.Repositories;

namespace Korp.Faturamento.Application.UseCases.NotasFiscais;

public class ImprimirNotaUseCase
{
    private readonly INotaFiscalRepository _notaRepository;
    private readonly IEstoqueService _estoqueService;

    public ImprimirNotaUseCase(
        INotaFiscalRepository notaRepository,
        IEstoqueService estoqueService)
    {
        _notaRepository = notaRepository;
        _estoqueService = estoqueService;
    }

    public async Task<NotaFiscalResponse> Execute(Guid id, Guid impressaoId)
    {
        var nota = await _notaRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Nota fiscal {id} não encontrada.");

        if (nota.JaProcessada(impressaoId))
            return MapearParaResponse(nota);

        var baixas = nota.Itens
            .Select(i => new BaixaEstoqueRequest(i.CodigoProduto, i.Quantidade))
            .ToList();

        await _estoqueService.BaixarEstoqueAsync(baixas);

        nota.Fechar(impressaoId);
        await _notaRepository.UpdateAsync(nota);

        return MapearParaResponse(nota);
    }

    private NotaFiscalResponse MapearParaResponse(NotaFiscal nota) => new NotaFiscalResponse(
        nota.Id,
        nota.Numero,
        nota.Status.ToString(),
        nota.CreatedAt,
        nota.Itens.Select(i => new ItemNotaResponse(i.Id, i.CodigoProduto, i.Quantidade)).ToList()
    );
}