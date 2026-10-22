namespace MinimalImplementation.Server.Player;

/// <summary>
/// Actual runtime collider and height sources, after mount PlayerSize delegation and resize.
/// Nullable reach values mean the effective runtime tile ranges have not been captured.
/// MountActive is runtime mount state, never an inference from packet 13 MountType.
/// </summary>
public sealed record PlayerSpatialSourceFacts(
  int Width,
  int Height,
  bool MountActive,
  int MountHeightBoost,
  bool MountUsesSizeOverride,
  bool PortableStoolInUse,
  int PortableStoolHeightBoost,
  int? TileRangeX,
  int? TileRangeY) {
  public int HeightOffsetBoost => MountActive
    ? MountHeightBoost : PortableStoolInUse ? PortableStoolHeightBoost : 0;
}
