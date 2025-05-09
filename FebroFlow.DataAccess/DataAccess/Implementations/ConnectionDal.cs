using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;
using febroFlow.DataAccess.DataAccess;

namespace febroFlow.DataAccess.DataAccess.Implementations
{
    /// <summary>
    /// Implementation of the Connection data access layer
    /// </summary>
    public class ConnectionDal : EfEntityRepositoryBase<Connection, DataContext>, IConnectionDal
    {
        /// <summary>
        /// Initializes a new instance of the ConnectionDal class
        /// </summary>
        /// <param name="context">The database context</param>
        public ConnectionDal(DataContext context) : base(context)
        {
        }
        
        // Implement any additional Connection-specific data access methods here
    }
}