using System.Numerics;
using Arch.Core;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileAttachmentComponent
{
  public Entity? AttachedTo;
  public ProjectileAttachmentKind Kind;
  public Vector2 LocalOffset;
  public long? AttachedTick;
  public ProjectileDetachPolicy DetachPolicy;
  public bool DeleteOnDetach;

  public bool IsAttached => AttachedTo.HasValue;
}
