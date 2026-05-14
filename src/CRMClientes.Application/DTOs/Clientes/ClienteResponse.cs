namespace CRMClientes.Application.DTOs.Clientes;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    string Documento,
    string? Endereco,
    string? Observacoes,
    bool Ativo,
    DateTime CreatedAt,
    DateTime UpdatedAt);
