namespace Korp.Estoque.Application.DTOs
{
    public record BaixaEstoqueRequest(
        string Codigo, 
        int Quantidade
    );
}
