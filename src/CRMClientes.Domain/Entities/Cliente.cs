using CRMClientes.Domain.Exceptions;

namespace CRMClientes.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;
    public string? Endereco { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private Cliente() { }

    public Cliente(
        string nome,
        string email,
        string telefone,
        string documento,
        Guid createdByUserId,
        string? endereco = null,
        string? observacoes = null)
    {
        ValidarCampos(nome, email, telefone, documento);

        if (createdByUserId == Guid.Empty)
            throw new DomainException("Usuário criador é obrigatório.");

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone.Trim();
        Documento = documento.Trim();
        Endereco = endereco?.Trim();
        Observacoes = observacoes?.Trim();
        Ativo = true;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Atualizar(
        string nome,
        string email,
        string telefone,
        string documento,
        string? endereco,
        string? observacoes)
    {
        ValidarCampos(nome, email, telefone, documento);

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        Telefone = telefone.Trim();
        Documento = documento.Trim();
        Endereco = endereco?.Trim();
        Observacoes = observacoes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Inativar()
    {
        Ativo = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidarCampos(string nome, string email, string telefone, string documento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("E-mail inválido.");

        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("Telefone é obrigatório.");

        if (string.IsNullOrWhiteSpace(documento))
            throw new DomainException("Documento (CPF/CNPJ) é obrigatório.");
    }
}
