using CRMClientes.Application.DTOs.Clientes;

namespace CRMClientes.Application.Interfaces;

public interface IClienteService
{
    Task<PagedResult<ClienteResponse>> ListarAsync(string? busca, int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ClienteResponse> CriarAsync(ClienteRequest request, Guid usuarioCriadorId, CancellationToken ct = default);
    Task<ClienteResponse> AtualizarAsync(Guid id, ClienteRequest request, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
}
