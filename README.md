# Backend — CRM-Clientes

API em **C# .NET 9** seguindo **Clean Architecture** (monolito), com **PostgreSQL** via **Entity Framework Core** e autenticação **JWT** com claims.

> Este README foi escrito pensando em quem está vendo Clean Architecture pela primeira vez. Se algo aqui não fizer sentido, o objetivo é que ele faça **depois de ler este documento**.

---

## Sumário

1. [O que é Clean Architecture, em uma analogia](#o-que-é-clean-architecture-em-uma-analogia)
2. [As 4 camadas deste projeto](#as-4-camadas-deste-projeto)
3. [Regra de dependência](#regra-de-dependência)
4. [Fluxo completo de uma requisição](#fluxo-completo-de-uma-requisição)
5. [Autenticação JWT explicada](#autenticação-jwt-explicada)
6. [Migrations](#migrations)
7. [Testes unitários](#testes-unitários)
8. [Pacotes NuGet usados](#pacotes-nuget-usados)
9. [Como rodar](#como-rodar)

---

## O que é Clean Architecture, em uma analogia

Pense em um **restaurante**:

- **Domain** = a receita do prato (o que é o prato, ingredientes essenciais, regras de qualidade). A receita não muda se você troca o fogão.
- **Application** = o chef (orquestra a receita, decide a ordem, junta tudo). Sabe o que fazer, mas não cozinha sozinho — pede ao auxiliar.
- **Infrastructure** = o auxiliar com fogão, geladeira, panelas (executa o trabalho técnico). Pode ser trocado: fogão a gás, elétrico, indução. A receita continua a mesma.
- **API** = o garçom (recebe pedidos do cliente, leva à cozinha, devolve o prato). Não cozinha — só conversa com o mundo externo.

Trocar a Infrastructure (de PostgreSQL para SQL Server, por exemplo) **não muda a receita**. Esse é o ponto da arquitetura.

---

## As 4 camadas deste projeto

```
backend/src/
├── CRMClientes.Domain/          ← núcleo, zero dependências externas
├── CRMClientes.Application/     ← casos de uso, depende só do Domain
├── CRMClientes.Infrastructure/  ← banco, JWT, BCrypt — implementa interfaces
└── CRMClientes.API/             ← Web API, monta tudo (Program.cs)
```

### 1. Domain (`CRMClientes.Domain`)

O **coração** do sistema. Não conhece banco, não conhece HTTP, não conhece JSON.

- `Entities/User.cs` e `Entities/Cliente.cs` — entidades com **regras de negócio no construtor** (ex.: `Cliente` exige nome, e-mail válido, documento, e quem o criou)
- `Enums/UserRole.cs` — papéis (hoje só `Admin`)
- `Exceptions/DomainException.cs` — para violações de regra
- `Interfaces/IUserRepository.cs`, `Interfaces/IClienteRepository.cs` — **contratos** que dizem o que o domínio **precisa** (mas a implementação fica em Infrastructure)

> Repare nos construtores das entidades: eles **lançam exceção** se receberem dados inválidos. Isso garante que **não existe entidade inválida** — uma regra forte de DDD.

### 2. Application (`CRMClientes.Application`)

Os **casos de uso**. Aqui é onde "registrar um usuário" e "criar um cliente" viram código.

- `DTOs/` — objetos de entrada e saída (`LoginRequest`, `ClienteResponse`, etc.)
- `Interfaces/IAuthService.cs`, `IClienteService.cs` — contratos dos serviços
- `Interfaces/IPasswordHasher.cs`, `IJwtTokenService.cs` — **abstrações** que Infrastructure vai implementar
- `Services/AuthService.cs` — orquestra: hash da senha, persistência, geração de token
- `Services/ClienteService.cs` — orquestra CRUD com paginação, busca, soft delete
- `Exceptions/` — exceções de aplicação (`NotFoundException`, `ConflictException`, `UnauthorizedException`)
- `Mapping/ClienteMapper.cs` — converte entidade para DTO
- `DependencyInjection.cs` — método `AddApplication()` que registra os services no DI

### 3. Infrastructure (`CRMClientes.Infrastructure`)

Os **detalhes técnicos**. Tudo que envolve "como" — banco, criptografia, JWT.

- `Persistence/AppDbContext.cs` — DbContext do EF Core
- `Persistence/Configurations/` — `UserConfiguration` e `ClienteConfiguration` definem mapeamento das tabelas (Fluent API)
- `Persistence/Repositories/` — implementam `IUserRepository` e `IClienteRepository` usando EF
- `Persistence/Migrations/` — migrations geradas pelo `dotnet ef`
- `Security/PasswordHasher.cs` — implementa `IPasswordHasher` usando BCrypt
- `Security/JwtTokenService.cs` — implementa `IJwtTokenService` (gera token, assina com HMAC-SHA256)
- `Security/JwtSettings.cs` — POCO carregado via `IOptions<>` da seção `Jwt` do `appsettings`
- `DependencyInjection.cs` — método `AddInfrastructure(configuration)` registra DbContext, repositórios, serviços de segurança

### 4. API (`CRMClientes.API`)

A camada que **fala com o mundo**. Web API ASP.NET Core.

- `Program.cs` — composição de tudo: DI, CORS, JWT Bearer, Swagger, middleware de exceção
- `Controllers/AuthController.cs` — `POST /api/auth/register` e `/login`
- `Controllers/ClientesController.cs` — CRUD com `[Authorize]` e leitura do `User.Id` via claim `NameIdentifier`
- `Middleware/ExceptionMiddleware.cs` — converte exceções tipadas em respostas HTTP corretas (404, 401, 409, 400)
- `appsettings.json` / `appsettings.Development.json` — connection string e configuração JWT

---

## Regra de dependência

```
API ──► Application ──► Domain
 │           ▲
 ▼           │
Infrastructure ──┘
```

- **Domain** não importa nada de fora.
- **Application** só importa de `Domain`.
- **Infrastructure** importa de `Domain` e `Application` (porque **implementa** as interfaces deles).
- **API** importa de `Application` e `Infrastructure` (compõe o DI no `Program.cs`).

> **Dica de leitura no editor:** abra qualquer `.csproj` e olhe os `<ProjectReference>`. Você nunca verá o Domain referenciando outra camada. Se um dia ver, é bug arquitetural.

---

## Fluxo completo de uma requisição

Vamos seguir um **`POST /api/clientes`** com JWT válido, passo a passo:

```
1. Request HTTP chega no Kestrel (servidor web do .NET)
   ↓
2. Pipeline ASP.NET Core: ExceptionMiddleware → CORS → Authentication → Authorization → Routing
   ↓
3. JwtBearer middleware lê o header "Authorization: Bearer ..." e valida o token
   ↓
4. ClientesController.Criar(request) é chamado
   ↓
5. Lê o User.Id do claim NameIdentifier (sub)
   ↓
6. Chama IClienteService.CriarAsync(request, userId)
   ↓
7. ClienteService instancia new Cliente(...) — o construtor valida tudo (Domain)
   ↓
8. ClienteService chama IClienteRepository.AdicionarAsync(cliente)
   ↓
9. ClienteRepository (Infrastructure) chama _context.Clientes.AddAsync()
   ↓
10. SalvarAlteracoesAsync() → EF gera SQL → INSERT no PostgreSQL
   ↓
11. Service mapeia Cliente → ClienteResponse
   ↓
12. Controller retorna 201 Created com o JSON da resposta
```

Se em qualquer ponto rolar uma exceção (`DomainException` se o e-mail for inválido, `UnauthorizedException` se o token for ruim), o `ExceptionMiddleware` captura e devolve o status HTTP correto.

---

## Autenticação JWT explicada

### O que é JWT?

JSON Web Token. É uma string em 3 partes separadas por `.`:

```
HEADER.PAYLOAD.SIGNATURE
```

- **Header**: tipo e algoritmo (ex.: `{"alg":"HS256","typ":"JWT"}`)
- **Payload**: as **claims** (afirmações sobre o usuário)
- **Signature**: assinatura HMAC com a chave secreta do servidor

A assinatura **não criptografa** — qualquer um pode decodificar o payload. Ela **garante que não foi alterado**, porque sem a chave secreta, ninguém consegue gerar uma assinatura válida.

### Claims que esta API gera

Veja `Infrastructure/Security/JwtTokenService.cs`:

```csharp
new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),       // identifica usuário
new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único do token
new(JwtRegisteredClaimNames.Email, user.Email),
new(ClaimTypes.NameIdentifier, user.Id.ToString()),          // .NET usa este p/ User.FindFirstValue
new(ClaimTypes.Name, user.Nome),
new(ClaimTypes.Email, user.Email),
new(ClaimTypes.Role, user.Role.ToString())                   // habilita [Authorize(Roles="Admin")]
```

Quando você usa `User.FindFirstValue(ClaimTypes.NameIdentifier)` em um Controller, está lendo o `sub` do token decodificado pelo middleware.

### Validação no `Program.cs`

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,        // o emissor é "CRMClientes.API"?
    ValidateAudience = true,      // a audiência é "CRMClientes.Frontend"?
    ValidateLifetime = true,      // o token está dentro do período de validade?
    ValidateIssuerSigningKey = true, // a assinatura é válida?
    ...
};
```

Se algum critério falhar, o middleware responde **401 Unauthorized** automaticamente — antes mesmo de chegar no Controller.

---

## Migrations

```bash
# criar uma nova migration
dotnet ef migrations add NomeDaMigration -p src/CRMClientes.Infrastructure -s src/CRMClientes.API

# aplicar pendentes no banco
dotnet ef database update -p src/CRMClientes.Infrastructure -s src/CRMClientes.API

# remover a última (só funciona se ainda não foi aplicada no banco)
dotnet ef migrations remove -p src/CRMClientes.Infrastructure -s src/CRMClientes.API
```

- `-p` (project): onde fica o `DbContext` e onde a migration vai ser gerada
- `-s` (startup): onde está o `Program.cs` (ele tem o `appsettings` com a connection string)

---

## Testes unitários

Stack: **xUnit + Moq + FluentAssertions**.

```
backend/tests/
├── CRMClientes.Domain.Tests/           ← 15 testes
│   └── Entities/
│       ├── UserTests.cs                ← validações do construtor
│       └── ClienteTests.cs             ← validações + Atualizar + Inativar
└── CRMClientes.Application.Tests/      ← 11 testes
    └── Services/
        ├── AuthServiceTests.cs         ← register, login, hash, JWT (com Moq)
        └── ClienteServiceTests.cs      ← CRUD, paginação, soft delete (com Moq)
```

### Exemplo comentado linha a linha

```csharp
[Fact] // marca como teste
public async Task Login_SenhaIncorreta_DeveLancarUnauthorized()
{
    // ARRANGE — preparar o cenário
    var user = new User("Itamar", "itamar@crm.local", "hash-real");

    // o repositório é um Mock — não é o EF de verdade
    _userRepoMock.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

    // qualquer chamada a Verify retorna false (senha errada)
    _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

    // ACT — executar o que estamos testando
    var act = async () => await CriarServico().LoginAsync(new LoginRequest("itamar@crm.local", "errada"));

    // ASSERT — verificar resultado (FluentAssertions deixa legível)
    await act.Should().ThrowAsync<UnauthorizedException>();
}
```

### Rodando os testes

```bash
cd backend
dotnet test                                     # todos
dotnet test tests/CRMClientes.Domain.Tests      # só domain
dotnet test --logger "console;verbosity=detailed"  # com saída detalhada
```

---

## Pacotes NuGet usados

| Pacote                                              | Onde       | Por que                                       |
| --------------------------------------------------- | ---------- | --------------------------------------------- |
| `Microsoft.EntityFrameworkCore`                     | Infra      | ORM principal                                 |
| `Npgsql.EntityFrameworkCore.PostgreSQL`             | Infra      | Provider PostgreSQL para EF Core              |
| `Microsoft.EntityFrameworkCore.Design`              | Infra, API | Necessário para o `dotnet ef` CLI funcionar   |
| `Microsoft.AspNetCore.Authentication.JwtBearer`     | API        | Middleware de validação de JWT                |
| `Microsoft.IdentityModel.Tokens`                    | Infra      | `SymmetricSecurityKey`, `SigningCredentials`  |
| `System.IdentityModel.Tokens.Jwt`                   | Infra      | `JwtSecurityTokenHandler` para escrever token |
| `BCrypt.Net-Next`                                   | Infra      | Hash de senha com salt automático             |
| `Swashbuckle.AspNetCore`                            | API        | Swagger UI com botão Authorize                |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | Infra   | `Configure<JwtSettings>(IConfigurationSection)` |
| `xunit`, `xunit.runner.visualstudio`                | Tests      | Framework de testes                           |
| `Moq`                                               | App.Tests  | Mock de interfaces                            |
| `FluentAssertions`                                  | Tests      | Asserções legíveis (`Should().Be(...)`)       |

---

## Como rodar

A partir da pasta `backend/`:

```bash
# 1. (uma vez) restaurar pacotes
dotnet restore

# 2. (uma vez) aplicar migrations no banco já rodando via Docker
dotnet ef database update -p src/CRMClientes.Infrastructure -s src/CRMClientes.API

# 3. subir a API
dotnet run --project src/CRMClientes.API
```

A API sobe em `http://localhost:5281` (a porta exata aparece no console). Acesse `http://localhost:5281/swagger` para o Swagger UI.

### Fluxo de teste rápido pelo Swagger

1. `POST /api/auth/register` com `{ "nome": "Admin", "email": "admin@crm.local", "password": "123456" }`
2. Copie o `token` da resposta
3. Clique em **Authorize** no canto superior do Swagger e cole o token
4. Teste `POST /api/clientes`, `GET /api/clientes`, etc.

---

📘 Frontend do projeto: [CRM-Clientes-Front](https://github.com/iLyzzi/CRM-Clientes-Front)
