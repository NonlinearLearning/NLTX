using System;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>
/// Stores the authoritative identity root for an entity instance.
/// </summary>
public readonly record struct EntityIdentityComponent {
  public EntityIdentityComponent(EntityUuid id) {
    if (!id.IsValid) {
      throw new ArgumentException("An entity identity requires a valid UUID.", nameof(id));
    }
    Id = id;
  }

  public EntityUuid Id { get; }

  public bool IsAssigned => Id.IsValid;
}
