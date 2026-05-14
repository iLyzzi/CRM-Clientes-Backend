using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Enums;
using CRMClientes.Domain.Exceptions;
using FluentAssertions;

namespace CRMClientes.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Construtor_DadosValidos_DeveCriarUsuario()
    {
        var user = new User("Itamar", "itamar@crm.local", "hash123");

        user.Id.Should().NotBeEmpty();
        user.Nome.Should().Be("Itamar");
        user.Email.Should().Be("itamar@crm.local");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be(UserRole.Admin);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construtor_DeveNormalizarEmailParaMinusculo()
    {
        var user = new User("Itamar", "  ITAMAR@CRM.LOCAL  ", "hash123");
        user.Email.Should().Be("itamar@crm.local");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_NomeVazio_DeveLancarDomainException(string nome)
    {
        var act = () => new User(nome, "itamar@crm.local", "hash123");
        act.Should().Throw<DomainException>().WithMessage("*nome*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    public void Construtor_EmailInvalido_DeveLancarDomainException(string email)
    {
        var act = () => new User("Itamar", email, "hash123");
        act.Should().Throw<DomainException>().WithMessage("*mail*");
    }

    [Fact]
    public void Construtor_PasswordHashVazio_DeveLancarDomainException()
    {
        var act = () => new User("Itamar", "itamar@crm.local", "");
        act.Should().Throw<DomainException>().WithMessage("*senha*");
    }
}
