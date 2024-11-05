using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Redirect task to todos
app.UseRewriter(new RewriteOptions().AddRedirect("task/(.*)", "/todos/$1"));

// Custom middleware to log the request method and path
app.Use(async (context, next) => {
    Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
    await next(context);
});

var todo = new List<Todo>();

// Get all todos
app.MapGet("/todos", () => todo);

// Get a todo by id
app.MapGet("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todo.SingleOrDefault(t => id == t.Id);
    return targetTodo is not null ? TypedResults.Ok(targetTodo) : TypedResults.NotFound();
});

// Create a todo
app.MapPost("/todos", (Todo task) => {
    todo.Add(task);
    return TypedResults.Created($"/todos/{task.Id}", task);
});

// Update a todo    
app.MapPut("/todos/{id}", Results<Ok<Todo>, NotFound> (int id, Todo task) => {
    var targetTodo = todo.SingleOrDefault(t => id == t.Id);
    if (targetTodo is not null)
    {
        var index = todo.IndexOf(targetTodo);
        todo[index] = task with { Id = id };
        return TypedResults.Ok(todo[index]); 
    }
    return TypedResults.NotFound();
});

// Delete a todo
app.MapDelete("/todos/{id}", Results<Ok<Todo>, NotFound> (int id) => {
    var targetTodo = todo.FirstOrDefault(t => id == t.Id);
    if (targetTodo is not null)
    {
        todo.Remove(targetTodo);
        return TypedResults.Ok(targetTodo);
    }
    return TypedResults.NotFound();
});

app.Run();

record Todo(int Id, string Name, DateTime DueDate, bool IsDone);