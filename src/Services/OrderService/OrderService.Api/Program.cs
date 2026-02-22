using OrderService.Api.Dependencies;
using OrderService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .WithServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
