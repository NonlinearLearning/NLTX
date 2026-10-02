using System;

namespace EntityEcs.Components;

public readonly record struct EntityIdentityComponent
{
  public EntityIdentityComponent(Guid uuid)
  {
    UUID = uuid;
  }

  public readonly Guid UUID;

  public static EntityIdentityComponent Create()
  {
    return new EntityIdentityComponent(Guid.NewGuid());
  }
}
