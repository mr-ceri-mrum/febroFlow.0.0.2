using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace febroFlow.DataAccess.DataAccess
{
    public interface IFlowDal : IEntityRepository<Flow>
    {
        // Add any Flow-specific data access methods here
    }
}