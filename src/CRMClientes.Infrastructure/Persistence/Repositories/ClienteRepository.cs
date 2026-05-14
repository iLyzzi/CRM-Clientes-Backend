using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMClientes.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken ct = default)
    {
        var query = AplicarBusca(_context.Clientes.AsNoTracking(), busca);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);
    }

    public Task<int> ContarAsync(string? busca, CancellationToken ct = default) =>
        AplicarBusca(_context.Clientes.AsNoTracking(), busca).CountAsync(ct);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default) =>
        await _context.Clientes.AddAsync(cliente, ct);

    public void Remover(Cliente cliente) => _context.Clientes.Remove(cliente);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    private static IQueryable<Cliente> AplicarBusca(IQueryable<Cliente> query, string? busca)
    {
        if (string.IsNullOrWhiteSpace(busca))
            return query;

        var termo = busca.Trim().ToLower();
        return query.Where(c =>
            c.Nome.ToLower().Contains(termo) ||
            c.Email.ToLower().Contains(termo) ||
            c.Documento.ToLower().Contains(termo));
    }
}
