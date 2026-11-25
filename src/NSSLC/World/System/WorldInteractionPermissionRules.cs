namespace Terraria.Unshared.World;

/// <summary>
/// Pure permission arithmetic for world interaction requests.
/// </summary>
/// <remarks>
/// <see cref="WorldInteractionSystem" /> authorizes entities and
/// <see cref="WorldInteractionCapabilityComponent" /> declares grants; both delegate the bit rules
/// here so no orchestration type carries them.
/// </remarks>
public static class WorldInteractionPermissionRules
{
  /// <summary>Every permission this build understands.</summary>
  public const WorldInteractionPermission KnownPermissions =
      WorldInteractionPermission.ReadTile |
      WorldInteractionPermission.BreakTile |
      WorldInteractionPermission.PlaceTile;

  /// <summary>Maps one requested operation onto the permission it requires.</summary>
  public static WorldInteractionPermission RequiredFor(WorldInteractionOperation operation)
  {
    return operation switch
    {
      WorldInteractionOperation.ReadTile => WorldInteractionPermission.ReadTile,
      WorldInteractionOperation.BreakTile => WorldInteractionPermission.BreakTile,
      WorldInteractionOperation.PlaceTile => WorldInteractionPermission.PlaceTile,
      _ => WorldInteractionPermission.None
    };
  }

  /// <summary>Rejects empty and unknown permission bits.</summary>
  public static bool IsKnown(WorldInteractionPermission permissions)
  {
    return permissions != WorldInteractionPermission.None &&
        (permissions & ~KnownPermissions) == WorldInteractionPermission.None;
  }

  /// <summary>True when every required bit is granted.</summary>
  public static bool Allows(
      WorldInteractionPermission granted,
      WorldInteractionPermission required)
  {
    return required != WorldInteractionPermission.None &&
        (granted & required) == required;
  }
}
