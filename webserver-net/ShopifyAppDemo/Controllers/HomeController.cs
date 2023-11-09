using DbMgr;
using DbMgr.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopifyAppDemo.Controllers;

[ApiController, Authorize]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDbMgr _dbMgr;

    public HomeController(ILogger<HomeController> logger, IDbMgr dbMgr)
    {
        _logger = logger;
        _dbMgr = dbMgr;
    }

    [HttpGet("healthcheck")]
    public string HealthCheck()
    {
        _logger.LogInformation("request received");
        return "ShopifyAppDemo is running!";
    }

    [HttpGet("installation/{id:int}")]
    public async Task<ActionResult<Installation>> GetInstallationById(int id)
    {
        var installation = await _dbMgr.GetInstallationByIdAsync(id);

        return installation == null ? NotFound() : Ok(installation);
    }
}
