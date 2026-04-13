namespace Korp.Estoque.Application.DTOs
{
    public record ProdutoRequest(
        string Codigo, 
        string Descricao, 
        int Saldo
    );
}
