using Arch.Core;

namespace Terraria.Dome.Simulation.Leash;

public struct LeashedEntityStateComponent
{
  public int InstanceId;
  public int TypeId;
  public Entity? Anchor;
  public int AnchorTileX;
  public int AnchorTileY;
  public bool Active;
  public LeashedEntityLifecycle LifecycleState;
  public bool Spawned;
}
