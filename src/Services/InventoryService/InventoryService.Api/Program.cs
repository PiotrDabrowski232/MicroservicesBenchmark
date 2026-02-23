using InventoryService.Api.Dependencies;
using InventoryService.Infrastructure.Dependencies;

using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


//Services injections
builder.Services.WithServices(builder.Configuration, builder.Environment);
builder.Services.InjectInfrastructure(builder.Configuration);



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
