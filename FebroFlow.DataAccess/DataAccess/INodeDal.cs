using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace febroFlow.DataAccess.DataAccess
{
    public interface INodeDal : IEntityRepository<Node>
    {
        // Add any Node-specific data access methods here
    }
}