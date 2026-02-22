using OrderService.Api.Dependencies;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .WithServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
