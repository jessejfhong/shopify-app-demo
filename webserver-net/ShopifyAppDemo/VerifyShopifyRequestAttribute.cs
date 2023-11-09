using ShopifyAppDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShopifySharp;

namespace ShopifyAppDemo;

/// <summary>
/// see https://shopify.dev/docs/apps/auth/oauth/getting-started#step-2-verify-the-installation-request
/// </summary>
public class VerifyShopifyRequestAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var configStore = ctx.HttpContext.RequestServices.GetService<IConfigStore>();

        // throw exception if configStore is null, or it's hard to know why it always fails
        var isAuthentic = AuthorizationService.IsAuthenticRequest(ctx.HttpContext.Request.Query, configStore?.ClientSecret ?? "");
        if (isAuthentic)
        {
            await next();
        }
        else
        {
            ctx.Result = new ForbidResult();
        }
    }
}
