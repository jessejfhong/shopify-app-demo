namespace ShopifyAppDemo.Services;

public class ConfigStore : IConfigStore
{
    public string ClientId { get; }
    public string ClientSecret { get; }
    public string AuthResultUrl { get; }
    public string[] ShopifyAccessPermissions { get; }

    public ConfigStore(string clientId, string clientSecret, string authResultUrl, string[] accessPermissions)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        AuthResultUrl = authResultUrl;
        ShopifyAccessPermissions = accessPermissions;
    }
}
