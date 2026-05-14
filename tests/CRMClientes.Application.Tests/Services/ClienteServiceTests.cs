using CRMClientes.Application.DTOs.Clientes;
using CRMClientes.Application.Exceptions;
using CRMClientes.Application.Services;
using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CRMClientes.Application.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _repoMock = new();
    private readonly Guid _userId = Guid.NewGuid();

    private ClienteService CriarServico() => new(_repoMock.Object);

    private ClienteRequest RequestValido() => new(
        "Empresa X",
        "contato@empresa.com",
        "11999998888",
        "12345678900",
        "Rua A, 123",
        null);

    [Fact]
    public async Task Criar_DadosValidos_DevePersistirComCreatedByUserIdCorreto()
    {
        Cliente? clienteCriado = null;
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Callback<Cliente, CancellationToken>((c, _) => clienteCriado = c)
            .Returns(Task.CompletedTask);

        var response = await CriarServico().CriarAsync(RequestValido(), _userId);

        clienteCriado.Should().NotBeNull();
        clienteCriado!.CreatedByUserId.Should().Be(_userId);
        response.Nome.Should().Be("Empresa X");
        response.Ativo.Should().BeTrue();
        _repoMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorId_NaoExiste_DeveLancarNotFound()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var act = async () => await CriarServico().ObterPorIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Atualizar_ClienteExistente_DeveAtualizarESalvar()
    {
        var cliente = new Cliente("Antigo", "old@x.com", "11111", "1234", _userId);
        _repoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var novoRequest = new ClienteRequest("Novo Nome", "new@x.com", "22222", "5678", null, null);
        var response = await CriarServico().AtualizarAsync(cliente.Id, novoRequest);

        response.Nome.Should().Be("Novo Nome");
        response.Email.Should().Be("new@x.com");
        cliente.CreatedByUserId.Should().Be(_userId, "porque o criador original não pode mudar em update");
        _repoMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remover_ClienteExistente_DeveInativarSemDeletarFisicamente()
    {
        var cliente = new Cliente("Empresa", "x@y.com", "111", "999", _userId);
        _repoMock.Setup(r => r.ObterPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        await CriarServico().RemoverAsync(cliente.Id);

        cliente.Ativo.Should().BeFalse();
        _repoMock.Verify(r => r.Remover(It.IsAny<Cliente>()), Times.Never);
        _repoMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Listar_DeveAplicarLimitesDePaginacao()
    {
        _repoMock.Setup(r => r.ListarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cliente>());
        _repoMock.Setup(r => r.ContarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await CriarServico().ListarAsync(null, pagina: -5, tamanhoPagina: 999);

        result.Pagina.Should().Be(1);
        result.TamanhoPagina.Should().Be(100);
    }
}
