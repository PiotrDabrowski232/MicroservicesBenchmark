using OrderService.Api.Dependencies;
using OrderService.Api.Middleware;
using OrderService.Infrastructure.Dependencies;

var builder = WebApplication.CreateBuilder(args);


//Services injections
builder.Services.WithServices(builder.Configuration, builder.Environment);
builder.Services.InjectInfrastructure(builder.Configuration);



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
