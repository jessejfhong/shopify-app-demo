using DbMgr.Entity;

namespace DbMgr;

public interface IDbMgr
{
    Task<Installation?> GetInstallationByIdAsync(int id);
    Task<Installation?> GetInstallationByDomainAsync(string domain);
    Task<IEnumerable<Installation>> GetInstallationAsync();
    Task SaveOrUpdateInstallationAsync(Installation installation);
    Task<int> DeleteInstallationByDomainAsync(string domain);
}
