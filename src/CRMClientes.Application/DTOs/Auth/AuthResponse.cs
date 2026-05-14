namespace CRMClientes.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiraEm,
    UserResponse Usuario);

public record UserResponse(Guid Id, string Nome, string Email, string Role);
