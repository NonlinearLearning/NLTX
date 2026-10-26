namespace Terraria.World;

/// <summary>Declares which world operations an entity is allowed to request.</summary>
public readonly record struct WorldInteractionCapabilityComponent(
    WorldInteractionPermission Permissions) {
  public bool Allows(WorldInteractionPermission permission) {
    return permission != WorldInteractionPermission.None &&
        (Permissions & permission) == permission;
  }
}
