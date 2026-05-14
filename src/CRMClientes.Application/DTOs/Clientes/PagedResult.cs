namespace CRMClientes.Application.DTOs.Clientes;

public record PagedResult<T>(
    IReadOnlyList<T> Itens,
    int Total,
    int Pagina,
    int TamanhoPagina);
