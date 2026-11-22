using System;
using Terraria.Relationships;

namespace Terraria.Physics;

/// <summary>Owns mount identity and the optional runtime mount entity relation.</summary>
public sealed class MountStateComponent
{
  public int MountKind { get; private set; } = -1;

  public EntityRuntimeHandle? MountEntity { get; private set; }

  public bool IsMounted => MountKind >= 0;

  public void Mount(int mountKind, EntityRuntimeHandle? mountEntity = null)
  {
    if (mountKind < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mountKind));
    }

    MountKind = mountKind;
    MountEntity = mountEntity;
  }

  public void Dismount()
  {
    MountKind = -1;
    MountEntity = null;
  }
}
