using CRMClientes.Application.DTOs.Auth;
using CRMClientes.Application.Exceptions;
using CRMClientes.Application.Interfaces;
using CRMClientes.Application.Services;
using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Enums;
using CRMClientes.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CRMClientes.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();

    private AuthService CriarServico() => new(_userRepoMock.Object, _hasherMock.Object, _jwtMock.Object);

    [Fact]
    public async Task Register_EmailNovo_DeveCriarUsuarioERetornarToken()
    {
        var request = new RegisterRequest("Itamar", "itamar@crm.local", "senha123");
        _userRepoMock.Setup(r => r.ExistePorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("senha123")).Returns("hash-fake");
        _jwtMock.Setup(j => j.GerarToken(It.IsAny<User>()))
            .Returns(("token-fake", DateTime.UtcNow.AddHours(1)));

        var result = await CriarServico().RegisterAsync(request);

        result.Token.Should().Be("token-fake");
        result.Usuario.Email.Should().Be("itamar@crm.local");
        result.Usuario.Role.Should().Be(nameof(UserRole.Admin));

        _userRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepoMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_EmailJaExistente_DeveLancarConflictException()
    {
        var request = new RegisterRequest("Itamar", "itamar@crm.local", "senha123");
        _userRepoMock.Setup(r => r.ExistePorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await CriarServico().RegisterAsync(request);
        await act.Should().ThrowAsync<ConflictException>();

        _userRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_SenhaCurta_DeveLancarConflictException()
    {
        var request = new RegisterRequest("Itamar", "itamar@crm.local", "12345");

        var act = async () => await CriarServico().RegisterAsync(request);
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*senha*");
    }

    [Fact]
    public async Task Login_CredenciaisValidas_DeveRetornarToken()
    {
        var user = new User("Itamar", "itamar@crm.local", "hash-real");
        _userRepoMock.Setup(r => r.ObterPorEmailAsync("itamar@crm.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("senha123", "hash-real")).Returns(true);
        _jwtMock.Setup(j => j.GerarToken(user)).Returns(("token-fake", DateTime.UtcNow.AddHours(1)));

        var result = await CriarServico().LoginAsync(new LoginRequest("itamar@crm.local", "senha123"));

        result.Token.Should().Be("token-fake");
        result.Usuario.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_DeveLancarUnauthorized()
    {
        _userRepoMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await CriarServico().LoginAsync(new LoginRequest("nao@existe.com", "senha"));
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_SenhaIncorreta_DeveLancarUnauthorized()
    {
        var user = new User("Itamar", "itamar@crm.local", "hash-real");
        _userRepoMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = async () => await CriarServico().LoginAsync(new LoginRequest("itamar@crm.local", "errada"));
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
