using DbMgr;
using DbMgr.Entity;
using ShopifyAppDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using ShopifySharp;
using System.Text;

namespace ShopifyAppDemo.Controllers;

[ApiController, Route("[controller]")]
public class ShopifyController : ControllerBase
{
    private readonly ILogger<ShopifyController> _logger;
    private readonly IDbMgr _dbMgr;
    private readonly IConfigStore _configStore;
    private readonly IDistributedCache _cache;

    public ShopifyController(
        ILogger<ShopifyController> logger,
        IDbMgr dbMgr,
        IConfigStore configStore,
        IDistributedCache cache)
    {
        _logger = logger;
        _dbMgr = dbMgr;
        _configStore = configStore;
        _cache = cache;
    }

    /* http://localhost:5155/shopify/handshake
     * ?hmac=6db16e9cb696ce73eb2d74594991f3ea2d26d3f1cae91b85f0719709b9a842d9
     * &host=YWRtaW4uc2hvcGlmeS5jb20vc3RvcmUvbG9vcGVyY29ycC1kZXY
     * &shop=loopercorp-dev.myshopify.com
     * &timestamp=1698493895
     */
    /// <summary>
    /// This is where users will be sent when they try to install the app, and when they try to login
    /// </summary>
    /// <param name="shop"></param>
    /// <returns></returns>
    [HttpGet("handshake"), VerifyShopifyRequest]
    public async Task<IActionResult> HandShakeAsync([FromQuery]string shop, [FromQuery]string host)
    {
        if (!await AuthorizationService.IsValidShopDomainAsync(shop))
        {
            var error = $"It looks like {shop} is not a valid Shopify shop domain.";
            _logger.LogError(error);
            return Problem(error, statusCode: 422);
        }

        // TODO: keep track of the scope this app requested, then ask for new permission
        // see https://shopify.dev/docs/apps/auth/oauth/getting-started#step-8-change-the-granted-scopes-optional
        // and https://shopify.dev/docs/apps/auth/oauth/session-tokens/getting-started#step-6-handle-changes-to-access-scopes-requested-by-your-app
        // redirect user to: https://{shop}.myshopify.com/admin/oauth/authorize ask for new permission

        /*
         * if the user is not signed in, show the user a asking a authorization screen,
         * present a button to request redirect url, do the redirection in client side
         */
        var record = await _dbMgr.GetInstallationByDomainAsync(shop);
        var isInstalled = record != null && !string.IsNullOrWhiteSpace(record.ShopifyAccessToken);
        if (isInstalled)
        {
            // When user open the installed app, redirect to home page
            // the client need to somehow check if user is login, then show home page or asking for authorization
            return LocalRedirect($"/?host={host}&shop={shop}");
        }
        else
        {
            // When user install the app, redirect to install page
            return Redirect(await GetAuthRedirectUrlAsync(shop));
        }
    }


    [Route("authurl")]
    public async Task<ActionResult<string>> GetRedirectUrlAsync(string shop)
    {
        var url = await GetAuthRedirectUrlAsync(shop);

        // this screen is where user authorize the app, after user clicking install, it will redirect user to /authresult
        return Ok(url);
    }

    private async Task<string> GetAuthRedirectUrlAsync(string shop)
    {
        var state = Guid.NewGuid().ToString();
        await _cache.SetAsync(state, Encoding.ASCII.GetBytes(state));

        // see detail from https://shopify.dev/docs/apps/auth/oauth/getting-started#step-3-request-authorization-code
        var oauthUrl = AuthorizationService.BuildAuthorizationUrl(
            _configStore.ShopifyAccessPermissions,
            shop,
            _configStore.ClientId,
            _configStore.AuthResultUrl,
            state);

        return oauthUrl.ToString();
    }

    /// <summary>
    /// see detail from https://shopify.dev/docs/apps/auth/oauth/getting-started#step-4-validate-authorization-code
    /// </summary>
    /// <param name="shop"></param>
    /// <param name="code"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    [HttpGet("authresult")]
    public async Task<IActionResult> AuthResultAsync([FromQuery] string shop, [FromQuery] string code, [FromQuery] string state, [FromQuery] string host)
    {
        var nonceBytes = await _cache.GetAsync(state);
        
        if (nonceBytes is null)
        {
            // the user need to go through the auth process again
            var url = await GetAuthRedirectUrlAsync(shop);
            return Redirect(url);
        }
        else
        {
            await _cache.RemoveAsync(state);
        }

        // Exchange the code param for a permanent Shopify access token
        var accessToken =
            await AuthorizationService.Authorize(code, shop, _configStore.ClientId, _configStore.ClientSecret);

        // Use the access token to get the user's shop info
        var shopService = new ShopService(shop, accessToken);
        var shopData = await shopService.GetAsync();

        var shopRecord = await _dbMgr.GetInstallationByDomainAsync(shop);
        if (shopRecord != null)
        {
            shopRecord.ShopifyShopDomain = shop;
            shopRecord.ShopifyAccessToken = accessToken;
            shopRecord.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            shopRecord = new Installation
            {
                ShopifyShopId = shopData.Id,
                ShopifyShopDomain = shop,
                ShopifyAccessToken = accessToken,
                UpdatedAt = DateTime.UtcNow
            };
        }

        await _dbMgr.SaveOrUpdateInstallationAsync(shopRecord);

        // or redirect using url provided here: https://shopify.dev/docs/apps/auth/oauth/getting-started#step-6-redirect-to-your-apps-ui
        // the app library also use this redirect url: https://{base64_decode(host)}/apps/{api_key}/
        // this way, we don't need app bridge to redirct back to shopify admin.
        //var decodedHost = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(host));
        //var redirectUrl = $"https://{decodedHost}/apps/{_configStore.ClientId}/?host={host}&shop={shop}";
        //return Redirect(redirectUrl);

        // set app bridge config "forceRedirect" to true will redirect the app back to embeded app
        return LocalRedirect($"/?host={host}&shop={shop}");
    }
}
