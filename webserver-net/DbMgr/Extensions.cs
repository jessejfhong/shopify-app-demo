using Microsoft.Extensions.DependencyInjection;

namespace DbMgr;

public static class Extensions
{
    public static void AddDbMgr(this IServiceCollection services, string? connectionString)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

        services.AddScoped<IDbMgr, DbMgr>(_ => new DbMgr(connectionString));
    }
}
