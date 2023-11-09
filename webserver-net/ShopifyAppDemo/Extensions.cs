using ShopifyAppDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;

namespace ShopifyAppDemo;

public static class Extensions
{
    /// <summary>
    /// Load the secret on starup, so it can throw exception during startup instead of unpon service request later.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddSecretMgr(this IServiceCollection services, IConfiguration config)
    {
        var clientId = config.GetValue<string>("ClientId");
        var clientSecret = config.GetValue<string>("ClientSecret");
        var authResultUrl = config.GetValue<string>("AuthResultUrl");
        var permissions = config.GetSection("ShopifyAccessPermissions").Get<string[]>();

        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException("ClientId config cannot be empty!");
        if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentNullException("ClientSecret config cannot be empty!");
        if (string.IsNullOrWhiteSpace(authResultUrl)) throw new ArgumentNullException("AuthResultUrl config cannot be empty!");
        if (permissions is null) throw new ArgumentNullException("ShopifyAccessPermissions config cannot be empty!");

        return services.AddSingleton<IConfigStore, ConfigStore>(_ => new ConfigStore(clientId, clientSecret, authResultUrl, permissions));
    }
}
