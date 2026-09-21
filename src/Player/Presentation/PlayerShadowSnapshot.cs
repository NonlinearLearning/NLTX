using System.Collections.ObjectModel;
using System.Numerics;

namespace Terraria.Player.Presentation;

public sealed class PlayerShadowSnapshot
{
  public PlayerShadowSnapshot(
    IReadOnlyList<Vector2> positions,
    IReadOnlyList<float> rotations,
    IReadOnlyList<Vector2> origins,
    IReadOnlyList<int> directions,
    int shadowCount,
    int availableAdvancedShadowsCount,
    int lastAddedAdvancedShadow,
    IReadOnlyList<PlayerAdvancedShadowSlot> advancedShadows,
    bool cursorItemIconReversed,
    int runSoundDelay,
    bool skipAnimatingValuesInPlayerFrame,
    PlayerCompositeArmSnapshot frontArm,
    PlayerCompositeArmSnapshot backArm)
  {
    Positions = Copy(positions);
    Rotations = Copy(rotations);
    Origins = Copy(origins);
    Directions = Copy(directions);
    AdvancedShadows = Copy(advancedShadows);
    ShadowCount = shadowCount;
    AvailableAdvancedShadowsCount = availableAdvancedShadowsCount;
    LastAddedAdvancedShadow = lastAddedAdvancedShadow;
    CursorItemIconReversed = cursorItemIconReversed;
    RunSoundDelay = runSoundDelay;
    SkipAnimatingValuesInPlayerFrame = skipAnimatingValuesInPlayerFrame;
    FrontArm = frontArm;
    BackArm = backArm;
  }

  public IReadOnlyList<Vector2> Positions { get; }

  public IReadOnlyList<float> Rotations { get; }

  public IReadOnlyList<Vector2> Origins { get; }

  public IReadOnlyList<int> Directions { get; }

  public IReadOnlyList<PlayerAdvancedShadowSlot> AdvancedShadows { get; }

  public int ShadowCount { get; }

  public int AvailableAdvancedShadowsCount { get; }

  public int LastAddedAdvancedShadow { get; }

  public bool CursorItemIconReversed { get; }

  public int RunSoundDelay { get; }

  public bool SkipAnimatingValuesInPlayerFrame { get; }

  public PlayerCompositeArmSnapshot FrontArm { get; }

  public PlayerCompositeArmSnapshot BackArm { get; }

  private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    return new ReadOnlyCollection<T>(values.ToArray());
  }
}
