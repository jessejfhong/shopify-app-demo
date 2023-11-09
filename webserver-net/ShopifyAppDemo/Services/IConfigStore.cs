namespace ShopifyAppDemo.Services;

public interface IConfigStore
{
    string ClientId { get; }
    string ClientSecret { get; }
    string AuthResultUrl { get; }
    string[] ShopifyAccessPermissions { get; }
}
