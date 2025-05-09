using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;
using febroFlow.DataAccess.DataAccess;

namespace febroFlow.DataAccess.DataAccess.Implementations
{
    /// <summary>
    /// Implementation of the Node data access layer
    /// </summary>
    public class NodeDal : EfEntityRepositoryBase<Node, DataContext>, INodeDal
    {
        /// <summary>
        /// Initializes a new instance of the NodeDal class
        /// </summary>
        /// <param name="context">The database context</param>
        public NodeDal(DataContext context) : base(context)
        {
        }
        
        // Implement any additional Node-specific data access methods here
    }
}