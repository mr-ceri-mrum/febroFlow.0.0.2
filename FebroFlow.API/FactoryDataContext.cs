using FebroFlow.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FebroFlow.API;

public class FactoryDataContext: IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            
        var builder = new DbContextOptionsBuilder<DataContext>();
        var connectionString = configurationRoot.GetConnectionString("DefaultConnection");
        
        builder.UseNpgsql(connectionString, b => 
            b.MigrationsAssembly("FebroFlow.API"));
        
        return new DataContext(builder.Options);
    }
}