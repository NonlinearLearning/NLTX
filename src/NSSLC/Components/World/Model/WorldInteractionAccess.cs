using Arch.Core;
using ArchEntity = Arch.Core.Entity;

namespace Terraria.World;

/// <summary>
/// A short-lived capability token issued after an entity passes a world permission check.
/// The wrapped Arch entity is intentionally inaccessible to callers.
/// </summary>
public readonly struct WorldInteractionAccess {
  private readonly ArchEntity _entity;
  private readonly WorldInteractionPermission _permissions;

  internal WorldInteractionAccess(ArchEntity entity, WorldInteractionPermission permissions) {
    _entity = entity;
    _permissions = permissions;
  }

  internal ArchEntity Entity => _entity;

  internal bool Allows(WorldInteractionPermission permission) {
    return permission != WorldInteractionPermission.None &&
        (_permissions & permission) == permission;
  }
}
