using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Exceptions;
using FluentAssertions;

namespace CRMClientes.Domain.Tests.Entities;

public class ClienteTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Construtor_DadosValidos_DeveCriarClienteAtivo()
    {
        var cliente = new Cliente(
            "Empresa X",
            "contato@empresa.com",
            "11999998888",
            "12345678900",
            _userId,
            endereco: "Rua A, 123",
            observacoes: "Cliente novo");

        cliente.Id.Should().NotBeEmpty();
        cliente.Nome.Should().Be("Empresa X");
        cliente.Email.Should().Be("contato@empresa.com");
        cliente.Ativo.Should().BeTrue();
        cliente.CreatedByUserId.Should().Be(_userId);
        cliente.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construtor_UsuarioCriadorVazio_DeveLancarDomainException()
    {
        var act = () => new Cliente(
            "Empresa X",
            "contato@empresa.com",
            "11999998888",
            "12345678900",
            Guid.Empty);

        act.Should().Throw<DomainException>().WithMessage("*riador*");
    }

    [Theory]
    [InlineData("", "contato@empresa.com", "11999", "12345", "*nome*")]
    [InlineData("Empresa X", "sem-arroba", "11999", "12345", "*mail*")]
    [InlineData("Empresa X", "contato@empresa.com", "", "12345", "*elefone*")]
    [InlineData("Empresa X", "contato@empresa.com", "11999", "", "*ocumento*")]
    public void Construtor_CamposInvalidos_DeveLancarDomainException(
        string nome, string email, string telefone, string documento, string mensagem)
    {
        var act = () => new Cliente(nome, email, telefone, documento, _userId);
        act.Should().Throw<DomainException>().WithMessage(mensagem);
    }

    [Fact]
    public void Inativar_DeveMarcarClienteComoInativoEAtualizarTimestamp()
    {
        var cliente = ClienteValido();
        var updatedAtAntes = cliente.UpdatedAt;
        Thread.Sleep(10);

        cliente.Inativar();

        cliente.Ativo.Should().BeFalse();
        cliente.UpdatedAt.Should().BeAfter(updatedAtAntes);
    }

    [Fact]
    public void Atualizar_DadosValidos_DeveAtualizarCamposManterIdEAtivo()
    {
        var cliente = ClienteValido();
        var idOriginal = cliente.Id;

        cliente.Atualizar(
            "Novo Nome",
            "novo@empresa.com",
            "21999",
            "98765432100",
            "Endereço novo",
            "Obs nova");

        cliente.Id.Should().Be(idOriginal);
        cliente.Nome.Should().Be("Novo Nome");
        cliente.Email.Should().Be("novo@empresa.com");
        cliente.Ativo.Should().BeTrue();
    }

    private Cliente ClienteValido() => new(
        "Empresa X",
        "contato@empresa.com",
        "11999998888",
        "12345678900",
        _userId);
}
