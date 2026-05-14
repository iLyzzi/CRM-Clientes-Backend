using System.Security.Claims;
using CRMClientes.Application.DTOs.Clientes;
using CRMClientes.Application.Exceptions;
using CRMClientes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMClientes.API.Controllers;

[ApiController]
[Authorize]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteResponse>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken ct = default)
    {
        var result = await _clienteService.ListarAsync(busca, pagina, tamanhoPagina, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var cliente = await _clienteService.ObterPorIdAsync(id, ct);
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(
        [FromBody] ClienteRequest request,
        CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        var cliente = await _clienteService.CriarAsync(request, usuarioId, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> Atualizar(
        Guid id,
        [FromBody] ClienteRequest request,
        CancellationToken ct)
    {
        var cliente = await _clienteService.AtualizarAsync(id, request, ct);
        return Ok(cliente);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await _clienteService.RemoverAsync(id, ct);
        return NoContent();
    }

    private Guid ObterUsuarioId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("Token sem identificador de usuário.");
        return Guid.Parse(idClaim);
    }
}
