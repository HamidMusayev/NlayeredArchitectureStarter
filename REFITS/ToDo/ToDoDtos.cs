namespace REFITS.ToDo;

/// <summary>
///     Response DTO returned by the sample ToDo API — maps the <c>id</c>, <c>title</c>, and
///     <c>completed</c> fields from the external service.
/// </summary>
public record ToDoToListDto
{
    public int ToDoId { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }
}

public record ToDoToAddDto
{
    public string Title { get; set; }
    public bool Completed { get; set; }
}

public record ToDoToUpdateDto
{
    public int ToDoId { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }
}