using DbMgr;

namespace ShopifyAppDemo;

public class HandleUserUninstall : BackgroundService
{
    private readonly ILogger<HandleUserUninstall> _logger;
    private readonly IServiceProvider _services;

    public HandleUserUninstall(
        ILogger<HandleUserUninstall> logger,
        IServiceProvider services)
    {
        _logger = logger;
        _services = services;
        
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    private async Task DoWork()
    {
        using  (var scope =  _services.CreateScope())
        {
            var dbMgr = scope.ServiceProvider.GetRequiredService<IDbMgr>();

            // delete the record
        }
    }
}
