using CRMClientes.Application.DTOs.Auth;
using CRMClientes.Application.Exceptions;
using CRMClientes.Application.Interfaces;
using CRMClientes.Domain.Entities;
using CRMClientes.Domain.Enums;
using CRMClientes.Domain.Interfaces;

namespace CRMClientes.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ConflictException("A senha deve ter no mínimo 6 caracteres.");

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistePorEmailAsync(emailNormalizado, ct))
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Nome, emailNormalizado, hash, UserRole.Admin);

        await _userRepository.AdicionarAsync(user, ct);
        await _userRepository.SalvarAlteracoesAsync(ct);

        return GerarResposta(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.ObterPorEmailAsync(request.Email.Trim().ToLowerInvariant(), ct)
            ?? throw new UnauthorizedException("Credenciais inválidas.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Credenciais inválidas.");

        return GerarResposta(user);
    }

    private AuthResponse GerarResposta(User user)
    {
        var (token, expiraEm) = _jwtTokenService.GerarToken(user);
        var userResponse = new UserResponse(user.Id, user.Nome, user.Email, user.Role.ToString());
        return new AuthResponse(token, expiraEm, userResponse);
    }
}
