using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;
using febroFlow.DataAccess.DataAccess;

namespace febroFlow.DataAccess.DataAccess.Implementations
{
    /// <summary>
    /// Implementation of the ExecutionState data access layer
    /// </summary>
    public class ExecutionStateDal : EfEntityRepositoryBase<ExecutionState, DataContext>, IExecutionStateDal
    {
        /// <summary>
        /// Initializes a new instance of the ExecutionStateDal class
        /// </summary>
        /// <param name="context">The database context</param>
        public ExecutionStateDal(DataContext context) : base(context)
        {
        }
        
        // Implement any additional ExecutionState-specific data access methods here
    }
}