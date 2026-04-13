namespace Korp.Estoque.Application.DTOs
{
    public record ProdutoResponse(
        Guid Id, 
        string Codigo, 
        string Descricao, 
        int Saldo
    );
}
