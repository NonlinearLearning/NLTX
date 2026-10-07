using Terraria.Relationships;

namespace EntityEcs.Components;

public readonly record struct EntityIdentityComponent
{
  internal EntityIdentityComponent(EntityUuid uuid)
  {
    if (!uuid.IsAssigned)
    {
      throw new ArgumentException("An entity identity component requires an assigned UUID.", nameof(uuid));
    }

    Uuid = uuid;
  }

  public EntityUuid Uuid { get; }
}
