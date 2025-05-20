using FebroFlow.DataAccess.DbModels;
using Microsoft.EntityFrameworkCore;

namespace FebroFlow.DataAccess.DbContexts
{
    public class DataContext : DbContext 
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        
        public DataContext() { }
        
        public DbSet<Flow> Flows { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<ExecutionState> ExecutionStates { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<ChatMemory> ChatMemories { get; set; }
        public DbSet<Vector> Vectors { get; set; }
        
    }
}