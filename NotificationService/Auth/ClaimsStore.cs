using System.Security.Claims;

namespace NotificationService.Auth;

public class ClaimsStore : IClaimsStore
{
    private List<Claim> _claims = new();
    private const string UserId = nameof(UserId);
    
    public ClaimsStore()
    {
    }

    private ClaimsStore(IEnumerable<Claim> claims)
    {
        _claims = claims.ToList();
    }

    public IClaimsStore FromClaims(IEnumerable<Claim> claims) =>
        new ClaimsStore(claims);

    public IClaimsStore AddUserId(long userId) =>
        new ClaimsStore(_claims.Concat(new[]
        {
            new Claim(UserId, userId.ToString())
        }));

    public long GetUserId() =>
        long.Parse(_claims.First(x => x.Type == UserId).Value);

    public IEnumerable<Claim> Claims =>
        _claims;
}