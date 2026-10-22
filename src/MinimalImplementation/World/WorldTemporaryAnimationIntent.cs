using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Immutable packet 77 input captured under the world owner gate. Version identifies the
/// capture; creating the intent does not advance it. The host owns encoding and broadcasting.
/// </summary>
public readonly record struct WorldTemporaryAnimationIntent(
    ulong WorldEpoch,
    CommitVersion Version,
    short AnimationType,
    ushort TileType,
    TileCoordinate Coordinate)
{
  public bool HasFrameDefinition => WorldTemporaryAnimationCatalog.TryGetFrames(
      AnimationType, out _, out _);
}

/// <summary>Server-side copy of Animation.SetDefaults used when a host tracks packet 77 effects.</summary>
public static class WorldTemporaryAnimationCatalog
{
  private static readonly IReadOnlyList<int> Type0Frames =
      Array.AsReadOnly(new[] { 1, 2, 3, 4, 5 });
  private static readonly IReadOnlyList<int> Type1Frames =
      Array.AsReadOnly(new[] { 5, 4, 3, 2, 1 });
  private static readonly IReadOnlyList<int> Type2Frames =
      Array.AsReadOnly(new[] { 1, 2, 2, 2, 1 });
  private static readonly IReadOnlyList<int> Type3Frames =
      Array.AsReadOnly(new[] { 0, 1, 2, 3, 4 });
  private static readonly IReadOnlyList<int> Type4Frames =
      Array.AsReadOnly(new[] { 9, 10, 11 });

  public static bool TryGetFrames(
      short animationType, out IReadOnlyList<int> frames, out int ticksPerFrame)
  {
    switch (animationType)
    {
      case 0:
        frames = Type0Frames;
        ticksPerFrame = 12;
        return true;
      case 1:
        frames = Type1Frames;
        ticksPerFrame = 12;
        return true;
      case 2:
        frames = Type2Frames;
        ticksPerFrame = 6;
        return true;
      case 3:
        frames = Type3Frames;
        ticksPerFrame = 5;
        return true;
      case 4:
        frames = Type4Frames;
        ticksPerFrame = 5;
        return true;
      default:
        frames = Array.Empty<int>();
        ticksPerFrame = 0;
        return false;
    }
  }
}
