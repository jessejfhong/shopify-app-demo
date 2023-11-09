using Dapper;
using DbMgr.Entity;
using Npgsql;

namespace DbMgr;

public class DbMgr : IDbMgr
{
    private readonly string _connStr;

    public DbMgr(string connStr)
    {
        _connStr = connStr;
    }

    private async Task<T> RunAsync<T>(Func<NpgsqlConnection, Task<T>> funcAsync)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connStr);
        await using var conn = await dataSource.OpenConnectionAsync();
        return await funcAsync(conn);
    }

    private async Task RunAsync(Func<NpgsqlConnection, Task> funcAsync)
    {
        await using var dataSource = NpgsqlDataSource.Create(_connStr);
        await using var conn = await dataSource.OpenConnectionAsync();
        await funcAsync(conn);
    }

    public async Task<Installation?> GetInstallationByIdAsync(int id)
    {
        var shops = await GetInstallationAsync();
        return shops.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// it makes sense to say that the shop is my customer, not the use who use it
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public async Task<Installation?> GetInstallationByDomainAsync(string domain)
    {
        var accounts = await GetInstallationAsync();
        return accounts.FirstOrDefault(a => a.ShopifyShopDomain == domain);
    }

    public async Task<IEnumerable<Installation>> GetInstallationAsync()
    {
        const string sql =
            @"select
                id Id,
                shopify_shop_id ShopifyShopId,
                shopify_shop_domain ShopifyShopDomain,
                shopify_access_token ShopifyAccessToken,
                updated_at UpdatedAt
              from installation";

        return await RunAsync(async conn => await conn.QueryAsync<Installation>(sql));
    }

    public async Task SaveOrUpdateInstallationAsync(Installation installation)
    {
        const string sqlNew =
            @"INSERT INTO
                installation (shopify_shop_id, shopify_shop_domain, shopify_access_token, updated_at)
                VALUES (@ShopifyShopId, @ShopifyShopDomain, @ShopifyAccessToken, @UpdatedAt)";
        const string sqlUpdate =
            @"UPDATE installation
                set shopify_shop_domain = @ShopifyShopDomain,
                    shopify_access_token = @ShopifyAccessToken,
                    updated_at = @UpdatedAt
             WHERE id = @Id";

        var sql = installation.Id == 0 ? sqlNew : sqlUpdate;

        await RunAsync(async conn => await conn.ExecuteAsync(sql, installation));
    }

    public async Task<int> DeleteInstallationByDomainAsync(string domain)
    {
        const string sql =
            @"DELETE FROM installation WHERE shopify_shop_domain = @Domain";

        return await RunAsync(async conn => await conn.ExecuteAsync(sql, new { Domain = domain }));
    }
}
