using CRMClientes.Application.DTOs.Clientes;
using CRMClientes.Application.Exceptions;
using CRMClientes.Application.Interfaces;
using CRMClientes.Application.Mapping;
using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Interfaces;

namespace CRMClientes.Application.Services;

public class ClienteService : IClienteService
{
    private const int TamanhoPaginaMaximo = 100;

    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<PagedResult<ClienteResponse>> ListarAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1) tamanhoPagina = 10;
        if (tamanhoPagina > TamanhoPaginaMaximo) tamanhoPagina = TamanhoPaginaMaximo;

        var clientes = await _clienteRepository.ListarAsync(busca, pagina, tamanhoPagina, ct);
        var total = await _clienteRepository.ContarAsync(busca, ct);

        var itens = clientes.Select(c => c.ToResponse()).ToList();
        return new PagedResult<ClienteResponse>(itens, total, pagina, tamanhoPagina);
    }

    public async Task<ClienteResponse> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Cliente {id} não encontrado.");
        return cliente.ToResponse();
    }

    public async Task<ClienteResponse> CriarAsync(
        ClienteRequest request,
        Guid usuarioCriadorId,
        CancellationToken ct = default)
    {
        var cliente = new Cliente(
            request.Nome,
            request.Email,
            request.Telefone,
            request.Documento,
            usuarioCriadorId,
            request.Endereco,
            request.Observacoes);

        await _clienteRepository.AdicionarAsync(cliente, ct);
        await _clienteRepository.SalvarAlteracoesAsync(ct);

        return cliente.ToResponse();
    }

    public async Task<ClienteResponse> AtualizarAsync(
        Guid id,
        ClienteRequest request,
        CancellationToken ct = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Cliente {id} não encontrado.");

        cliente.Atualizar(
            request.Nome,
            request.Email,
            request.Telefone,
            request.Documento,
            request.Endereco,
            request.Observacoes);

        await _clienteRepository.SalvarAlteracoesAsync(ct);
        return cliente.ToResponse();
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Cliente {id} não encontrado.");

        cliente.Inativar();
        await _clienteRepository.SalvarAlteracoesAsync(ct);
    }
}
