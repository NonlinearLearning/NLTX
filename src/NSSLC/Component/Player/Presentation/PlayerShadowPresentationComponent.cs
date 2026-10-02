using System.Numerics;

namespace Terraria.Player.Presentation;

public sealed class PlayerShadowPresentationComponent
{
  public const int SocialShadowCapacity = 3;

  public const int AdvancedShadowCapacity = 60;

  private readonly Vector2[] _shadowPositions = new Vector2[SocialShadowCapacity];

  private readonly float[] _shadowRotations = new float[SocialShadowCapacity];

  private readonly Vector2[] _shadowOrigins = new Vector2[SocialShadowCapacity];

  private readonly int[] _shadowDirections = new int[SocialShadowCapacity];

  private readonly PlayerAdvancedShadowSlot[] _advancedShadows =
    new PlayerAdvancedShadowSlot[AdvancedShadowCapacity];

  public bool CursorItemIconReversed { get; private set; }

  public int RunSoundDelay { get; private set; }

  public int ShadowCount { get; private set; }

  public bool SkipAnimatingValuesInPlayerFrame { get; private set; }

  public int AvailableAdvancedShadowsCount { get; private set; }

  public int LastAddedAdvancedShadow { get; private set; }

  internal void ApplyInput(in PlayerShadowPresentationInput input)
  {
    CursorItemIconReversed = input.CursorItemIconReversed;
    RunSoundDelay = Math.Max(0, input.RunSoundDelay);
    SkipAnimatingValuesInPlayerFrame = input.SkipAnimatingValuesInPlayerFrame;

    if (input.ResetSocialShadow)
    {
      ResetSocialShadow();
    }

    if (input.AddAdvancedShadow && input.AdvancedShadow.HasValue)
    {
      AddAdvancedShadow(input.AdvancedShadow.Value);
    }

    UpdateSocialShadow(
      input.Position,
      input.GfxOffY,
      input.FullRotation,
      input.FullRotationOrigin,
      input.Direction);
  }

  internal void ResetAdvancedShadows()
  {
    AvailableAdvancedShadowsCount = 0;
  }

  internal void ResetSocialShadow()
  {
    ShadowCount = 0;
  }

  internal void ResetForLifecycle(in PlayerShadowPresentationInput input)
  {
    ResetAdvancedShadows();
    ResetSocialShadow();
    for (int index = 0; index < SocialShadowCapacity; index++)
    {
      UpdateSocialShadow(
        input.Position,
        input.GfxOffY,
        input.FullRotation,
        input.FullRotationOrigin,
        input.Direction);
    }
  }

  internal void ClearForRemoval()
  {
    Array.Clear(_shadowPositions, 0, _shadowPositions.Length);
    Array.Clear(_shadowRotations, 0, _shadowRotations.Length);
    Array.Clear(_shadowOrigins, 0, _shadowOrigins.Length);
    Array.Clear(_shadowDirections, 0, _shadowDirections.Length);
    Array.Clear(_advancedShadows, 0, _advancedShadows.Length);
    CursorItemIconReversed = false;
    RunSoundDelay = 0;
    ShadowCount = 0;
    SkipAnimatingValuesInPlayerFrame = false;
    AvailableAdvancedShadowsCount = 0;
    LastAddedAdvancedShadow = 0;
  }

  public PlayerShadowSnapshot ToSnapshot()
  {
    return new PlayerShadowSnapshot(
      _shadowPositions,
      _shadowRotations,
      _shadowOrigins,
      _shadowDirections,
      ShadowCount,
      AvailableAdvancedShadowsCount,
      LastAddedAdvancedShadow,
      _advancedShadows,
      CursorItemIconReversed,
      RunSoundDelay,
      SkipAnimatingValuesInPlayerFrame);
  }

  private void AddAdvancedShadow(PlayerAdvancedShadowSlot shadow)
  {
    AvailableAdvancedShadowsCount =
      Math.Min(AdvancedShadowCapacity, AvailableAdvancedShadowsCount + 1);
    LastAddedAdvancedShadow++;
    if (LastAddedAdvancedShadow >= AdvancedShadowCapacity)
    {
      LastAddedAdvancedShadow = 0;
    }

    _advancedShadows[LastAddedAdvancedShadow] = shadow with
    {
      SlotIndex = LastAddedAdvancedShadow,
    };
  }

  private void UpdateSocialShadow(
    Vector2 position,
    float gfxOffY,
    float fullRotation,
    Vector2 fullRotationOrigin,
    int direction)
  {
    for (int index = SocialShadowCapacity - 1; index > 0; index--)
    {
      _shadowDirections[index] = _shadowDirections[index - 1];
    }

    _shadowDirections[0] = direction;
    ShadowCount++;
    if (ShadowCount == 1)
    {
      _shadowPositions[2] = _shadowPositions[1];
      _shadowRotations[2] = _shadowRotations[1];
      _shadowOrigins[2] = _shadowOrigins[1];
    }
    else if (ShadowCount == 2)
    {
      _shadowPositions[1] = _shadowPositions[0];
      _shadowRotations[1] = _shadowRotations[0];
      _shadowOrigins[1] = _shadowOrigins[0];
    }
    else if (ShadowCount >= SocialShadowCapacity)
    {
      ShadowCount = 0;
      _shadowPositions[0] = position + new Vector2(0f, gfxOffY);
      _shadowRotations[0] = fullRotation;
      _shadowOrigins[0] = fullRotationOrigin;
    }
  }
}
