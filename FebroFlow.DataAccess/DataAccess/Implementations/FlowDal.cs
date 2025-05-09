using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;
using febroFlow.DataAccess.DataAccess;

namespace febroFlow.DataAccess.DataAccess.Implementations
{
    /// <summary>
    /// Implementation of the Flow data access layer
    /// </summary>
    public class FlowDal : EfEntityRepositoryBase<Flow, DataContext>, IFlowDal
    {
        /// <summary>
        /// Initializes a new instance of the FlowDal class
        /// </summary>
        /// <param name="context">The database context</param>
        public FlowDal(DataContext context) : base(context)
        {
        }
        
        // Implement any additional Flow-specific data access methods here
    }
}