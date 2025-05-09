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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<Node>()
                .HasOne(n => n.Flow)
                .WithMany(f => f.Nodes)
                .HasForeignKey(n => n.FlowId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Flow)
                .WithMany(f => f.Connections)
                .HasForeignKey(c => c.FlowId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.SourceNode)
                .WithMany()
                .HasForeignKey(c => c.SourceNodeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.TargetNode)
                .WithMany()
                .HasForeignKey(c => c.TargetNodeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}