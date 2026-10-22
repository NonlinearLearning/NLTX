namespace MinimalImplementation.Server.Player;

/// <summary>Authoritative runtime collider, mount and item-use state owned by PlayerOwner.</summary>
public sealed record PlayerRuntimeSpatialState(
  int Width,
  int Height,
  bool MountActive,
  ushort? ActiveMountType,
  int MountHeightBoost,
  bool MountUsesSizeOverride,
  bool PortableStoolHas,
  bool PortableStoolInUse,
  bool PortableStoolStateKnown,
  bool Pulley,
  bool? GravityControlled,
  bool GrapplingStateKnown,
  MinimalImplementation.Server.Identity.EntityHandle? FirstGrapplingProjectile,
  int ItemAnimation,
  bool ItemAnimationKnown,
  bool VoidVaultEnabled)
{
  public static PlayerRuntimeSpatialState Initial => new(20, 42, false, null, 0, false,
    false, false, true, false, null, false, null, 0, true, false);

  public int HeightOffsetBoost => MountActive
    ? MountHeightBoost
    : PortableStoolInUse ? 26 : 0;
}
