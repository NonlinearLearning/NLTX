namespace Terraria.WorldSession.Queries;

public sealed class NpcShopSessionState
{
  public int ShopId { get; private set; }

  public bool IsOpen { get; private set; }

  public void Open(int shopId)
  {
    if (shopId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(shopId));
    }

    ShopId = shopId;
    IsOpen = true;
  }

  public void Close()
  {
    IsOpen = false;
    ShopId = 0;
  }
}
