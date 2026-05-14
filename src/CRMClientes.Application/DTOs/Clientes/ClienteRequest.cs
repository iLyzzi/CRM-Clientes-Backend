namespace CRMClientes.Application.DTOs.Clientes;

public record ClienteRequest(
    string Nome,
    string Email,
    string Telefone,
    string Documento,
    string? Endereco,
    string? Observacoes);
