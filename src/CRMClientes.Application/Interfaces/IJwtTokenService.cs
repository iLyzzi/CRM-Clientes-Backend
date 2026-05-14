using CRMClientes.Domain.Entities;

namespace CRMClientes.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(User user);
}
