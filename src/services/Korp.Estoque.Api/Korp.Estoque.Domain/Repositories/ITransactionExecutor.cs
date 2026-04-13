namespace Korp.Estoque.Domain.Repositories;

public interface ITransactionExecutor
{
    Task ExecuteAsync(Func<Task> operation);
}