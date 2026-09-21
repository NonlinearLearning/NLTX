namespace Terraria.WorldInteraction.Components;

public sealed class ManEaterProtectionSystem
{
  public void BeginFrame(ManEaterProtectionIndexComponent index)
  {
    ArgumentNullException.ThrowIfNull(index);
    index.Clear();
  }

  public void ProtectSpot(ManEaterProtectionIndexComponent index, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(index);
    index.Protect(x, y);
  }

  public bool SpotProtected(ManEaterProtectionIndexComponent index, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(index);
    return index.Contains(x, y);
  }
}
