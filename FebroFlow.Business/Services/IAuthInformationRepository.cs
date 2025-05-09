using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FebroFlow.Business.Services;

public interface IAuthInformationRepository
{
    IdentityAuthModel? GetUser();
    IdentityAuthModel? GetUserId();
}
public class AuthInformationRepository(IHttpContextAccessor httpContextAccessor) : IAuthInformationRepository
{
    public IdentityAuthModel? GetUser()
    {
        var context = httpContextAccessor.HttpContext?.User;
        var identities = context?.Identities.FirstOrDefault();
        if (identities == null)
            return null;

        var claims = identities.Claims.ToList();
        if (!claims.Any())
            return null;

        return new IdentityAuthModel()
        {
            Id = Guid.Parse(claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value!),
            UserName = claims.FirstOrDefault(i => i.Type == ClaimTypes.Email)?.Value ?? "",
        };
    }

    public IdentityAuthModel? GetUserId()
    {
        var context = httpContextAccessor.HttpContext?.User;
        var identities = context?.Identities.FirstOrDefault();
        if (identities == null)
            return null;

        var claims = identities.Claims.ToList();
        if (!claims.Any())
            return null;

        return new IdentityAuthModel()
        {
            Id = Guid.Parse(claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value!),
            UserName = claims.FirstOrDefault(i => i.Type == ClaimTypes.Email)?.Value ?? "",
        };
    }
}

public class IdentityAuthModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
}