namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// One captured tile and its type-set facts. IsActive means nactive (active and not actuated).
/// Half-block and slope bits are retained even on inactive tiles, as in AI_003's step checks.
/// </summary>
public sealed record NpcFighterTile(
    bool IsActive,
    bool IsSolid,
    bool IsSolidTop,
    bool IsHalfBlock,
    bool IsTopSlope,
    bool IsPlatform,
    bool HasProperPlatformTopFrame,
    bool IsClosedDoorOrGate) {
  internal bool SolidWithoutPlatforms => IsActive && !IsPlatform && (IsSolid || IsSolidTop);

  internal bool SolidAllowBottomSlope => IsActive && (IsSolid || IsSolidTop) &&
      (!IsTopSlope || (IsPlatform && HasProperPlatformTopFrame)) && !IsHalfBlock;

  internal bool BlocksStep => IsActive && IsSolid && !IsSolidTop;
}
