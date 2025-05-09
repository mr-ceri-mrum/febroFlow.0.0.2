using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace FebroFlow.DataAccess.DataAccess;

public interface IChatMemoryDal : IEntityRepository<ChatMemory> { }

public class ChatMemoryDal : EfEntityRepositoryBase<ChatMemory, DataContext>, IChatMemoryDal
{
    public ChatMemoryDal(DataContext context) : base(context)
    {
    }
}