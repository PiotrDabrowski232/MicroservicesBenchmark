using InventoryService.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 2, 0));
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var logger = services.GetRequiredService<ILogger<Program>>();
        await InventoryDbContextSeed.SeedAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Db Error");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
