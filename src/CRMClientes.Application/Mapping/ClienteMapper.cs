using CRMClientes.Application.DTOs.Clientes;
using CRMClientes.Domain.Entities;

namespace CRMClientes.Application.Mapping;

internal static class ClienteMapper
{
    public static ClienteResponse ToResponse(this Cliente cliente) => new(
        cliente.Id,
        cliente.Nome,
        cliente.Email,
        cliente.Telefone,
        cliente.Documento,
        cliente.Endereco,
        cliente.Observacoes,
        cliente.Ativo,
        cliente.CreatedAt,
        cliente.UpdatedAt);
}
