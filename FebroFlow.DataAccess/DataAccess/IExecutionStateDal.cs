using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace febroFlow.DataAccess.DataAccess
{
    public interface IExecutionStateDal : IEntityRepository<ExecutionState>
    {
        // Add any ExecutionState-specific data access methods here
    }
}