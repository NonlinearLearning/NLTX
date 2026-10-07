namespace Terraria.Player;

public sealed class PlayerBarrierFrameProjection
{
  public PlayerBarrierFrameSnapshot Snapshot(
    PlayerBarrierAndRegenComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);

    return new PlayerBarrierFrameSnapshot(
      component.IceBarrier,
      component.IceBarrierFrame,
      component.IceBarrierFrameCounter);
  }
}
