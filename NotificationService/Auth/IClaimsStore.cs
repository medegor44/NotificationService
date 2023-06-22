using System.Security.Claims;

namespace NotificationService.Auth;

public interface IClaimsStore
{
    IClaimsStore FromClaims(IEnumerable<Claim> claims);
    IClaimsStore AddUserId(long userId);
    long GetUserId();
    IEnumerable<Claim> Claims { get; }
}