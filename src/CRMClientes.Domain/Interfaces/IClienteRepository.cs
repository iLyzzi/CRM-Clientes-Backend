using CRMClientes.Domain.Entities;

namespace CRMClientes.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(string? busca, int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<int> ContarAsync(string? busca, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    void Remover(Cliente cliente);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
