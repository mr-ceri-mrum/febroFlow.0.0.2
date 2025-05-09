using FebroFlow.DataAccess.DbContexts;
using FebroFlow.DataAccess.DbModels;
using FebroFlow.DataAccess.EfEntityRepositoryBase;

namespace FebroFlow.DataAccess.DataAccess;

public interface ICredentialDal : IEntityRepository<Credential> { }

public class CredentialDal : EfEntityRepositoryBase<Credential, DataContext>, ICredentialDal
{
    public CredentialDal(DataContext context) : base(context)
    {
    }
}