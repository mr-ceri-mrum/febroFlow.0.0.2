using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace febroFlow.DataAccess.DataAccess.Implementations;

public class VectorDal : EfEntityRepositoryBase<Vector, DataContext>, IVectorDal
{
    public VectorDal(DataContext context) : base(context)
    {
    }
}