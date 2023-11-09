namespace DbMgr.Entity;

public class Installation
{
    public int Id { get; set; }
    public long? ShopifyShopId { get; set; }
    public string ShopifyShopDomain { get; set; } = null!;
    public string ShopifyAccessToken { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
}
