using CRMClientes.Domain.Entities;

namespace CRMClientes.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistePorEmailAsync(string email, CancellationToken ct = default);
    Task AdicionarAsync(User user, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
