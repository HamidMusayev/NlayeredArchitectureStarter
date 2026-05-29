using Refit;

namespace REFITS.ToDo;

/// <summary>
///     Refit-generated HTTP client for the sample ToDo REST API. Demonstrates how to wire
///     a strongly-typed REST client with Refit; swap the base URL via <c>ToDoClientSettings</c>
///     in <c>appsettings.json</c>. Not production data — for boilerplate demonstration only.
/// </summary>
public interface IToDoClient
{
    [Get("/todo")]
    Task<List<ToDoToListDto>> Get();

    [Get("/todo/{id}")]
    Task<ToDoToListDto> Get(int id);

    [Post("/todo")]
    Task<ToDoToListDto> Create([Body] ToDoToAddDto todo);

    [Put("/todo/{id}")]
    Task<ToDoToListDto> Update(int id, [Body] ToDoToUpdateDto todo);

    [Delete("/todo/{id}")]
    Task Delete(int id);
}