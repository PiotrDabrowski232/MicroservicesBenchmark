using PaymentService.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
