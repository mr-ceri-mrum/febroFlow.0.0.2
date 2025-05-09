using FebroFlow.Business.ServiceRegistrations;
using FebroFlow.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register all services from the Business layer
builder.Services.AddServices(builder.Configuration, builder.Environment);

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder.Configuration.GetSection("CorsLabel").Value!);

app.UseAuthorization();

app.MapControllers();

// Automatically run migrations if needed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    
    var appliedMigrations = dbContext.Database.GetAppliedMigrations();
    var pendingMigrations = dbContext.Database.GetPendingMigrations();
    var missingMigrations = pendingMigrations.Except(appliedMigrations).ToList();
        
    if (missingMigrations.Any())
    {
        Console.WriteLine("There are pending migrations:");
        foreach (var migration in missingMigrations)
        {
            Console.WriteLine(migration);
        }
        
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    else
    {
        Console.WriteLine("All migrations are up-to-date.");
    }
}

app.Run();
