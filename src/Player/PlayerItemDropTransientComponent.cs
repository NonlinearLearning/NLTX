namespace Terraria.Player;

public sealed class PlayerItemDropTransientComponent
{
  public bool JustDroppedAnItem { get; internal set; }

  internal void ResetForLifecycle()
  {
    JustDroppedAnItem = false;
  }
}
