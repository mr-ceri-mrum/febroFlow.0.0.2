using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace febroFlow.DataAccess.DataAccess
{
    public interface IConnectionDal : IEntityRepository<Connection>
    {
        // Add any Connection-specific data access methods here
    }
}