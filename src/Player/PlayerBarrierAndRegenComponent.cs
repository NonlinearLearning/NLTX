namespace Terraria.Player;

public sealed class PlayerBarrierAndRegenComponent
{
  public bool IceBarrier { get; internal set; }

  public byte IceBarrierFrame { get; internal set; }

  public byte IceBarrierFrameCounter { get; internal set; }

  public bool PalladiumRegen { get; internal set; }
}
