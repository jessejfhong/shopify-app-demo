using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopifyAppDemo;

public class ShopifySessionTokenValidator : JwtSecurityTokenHandler
{
    public override ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
    {
        // validate this token: https://shopify.dev/docs/apps/auth/oauth/session-tokens/getting-started#step-3-decode-session-tokens-for-incoming-requests
        var principal = base.ValidateToken(securityToken, validationParameters, out validatedToken);

        var token = ReadJwtToken(securityToken);

        var iss = token.Payload.Iss;
        var dest = token.Claims.FirstOrDefault(c => c.Type == "dest");

        var isSame = new Uri(iss).Host == new Uri(dest?.Value ?? string.Empty).Host;
        if (!isSame)
            throw new Exception($"top-level domain {iss} and {dest} doesn't match!");
        
        return principal;
    }
}
