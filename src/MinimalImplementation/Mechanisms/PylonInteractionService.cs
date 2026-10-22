using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public sealed record PylonNpcFact(
  bool Active,
  bool IsLikeTownNpc,
  bool Homeless,
  TileCoordinate Home,
  double CenterTileX,
  double CenterTileY);

public sealed record PylonEnvironmentFacts(
  bool SurfacePurity,
  bool Jungle,
  bool Hallow,
  bool Underground,
  bool Beach,
  bool Desert,
  bool Snow,
  bool GlowingMushroom,
  bool Victory,
  bool Underworld,
  bool Shimmer,
  bool TempleWall,
  double WorldSurface,
  double RockLayer,
  int MaxTilesY,
  int MaxTilesX,
  bool RemixWorld,
  bool DownedPlantBoss)
{
  /// <summary>Maps one P03 capture and the P07 progression fact without defaulting either.</summary>
  public static PylonEnvironmentFacts FromScene(
    World.WorldSceneMetrics scene,
    bool downedPlantBoss)
  {
    ArgumentNullException.ThrowIfNull(scene);
    var anchor = new TileCoordinate(scene.Anchor.X, scene.Anchor.Y);
    double surface = scene.Header.Facts.WorldSurface;
    double rockLayer = scene.Header.Facts.RockLayer;
    int height = scene.Header.Geometry.Height;
    int width = scene.Header.Geometry.Width;
    bool remix = scene.Header.Facts.RemixWorld;
    return new PylonEnvironmentFacts(
      scene.SurfacePurity,
      scene.Jungle,
      scene.Hallow,
      anchor.Y >= surface,
      IsBeachPosition(anchor, surface, rockLayer, width, height, remix),
      scene.Desert,
      scene.Snow,
      scene.GlowingMushroom,
      Victory: true,
      Underworld: anchor.Y >= height - 200,
      scene.Shimmer,
      scene.TempleWall,
      surface,
      rockLayer,
      height,
      width,
      remix,
      downedPlantBoss);
  }

  public bool IsValid(byte pylonType, TileCoordinate anchor)
  {
    if (anchor.X < 0 || anchor.Y < 0 || anchor.X >= MaxTilesX || anchor.Y >= MaxTilesY ||
        !double.IsFinite(WorldSurface) || !double.IsFinite(RockLayer))
    {
      return false;
    }

    if (pylonType == 8)
    {
      return true;
    }

    return pylonType switch
    {
      0 => SurfacePurity && IsSurfacePurityPosition(anchor),
      1 => Jungle,
      2 => Hallow,
      3 => anchor.Y >= WorldSurface,
      4 => IsBeachPosition(anchor, WorldSurface, RockLayer, MaxTilesX, MaxTilesY, RemixWorld),
      5 => Desert,
      6 => Snow,
      7 => GlowingMushroom && (!RemixWorld || anchor.Y < MaxTilesY - 200),
      9 => anchor.Y >= MaxTilesY - 200,
      10 => Shimmer,
      _ => false
    };
  }

  private bool IsSurfacePurityPosition(TileCoordinate anchor)
  {
    bool correctDepth = RemixWorld
      ? anchor.Y > RockLayer && anchor.Y < MaxTilesY - 350
      : anchor.Y <= WorldSurface;
    bool coast = anchor.X <= 380 || anchor.X >= MaxTilesX - 380;
    return correctDepth && !coast;
  }

  private static bool IsBeachPosition(
    TileCoordinate anchor,
    double worldSurface,
    double rockLayer,
    int width,
    int height,
    bool remixWorld)
  {
    bool correctDepth = anchor.Y <= worldSurface &&
      anchor.Y > worldSurface * 0.3499999940395355;
    bool coast = anchor.X <= 380 || anchor.X >= width - 380;
    if (remixWorld)
    {
      correctDepth |= anchor.Y > rockLayer && anchor.Y < height - 350;
      coast |= anchor.X < width * 0.43 || anchor.X > width * 0.57;
    }
    return correctDepth && coast;
  }
}

public enum PylonTeleportFailure
{
  None,
  TargetMissing,
  TargetEnvironment,
  TargetNpcCount,
  AccessingTempleEarly,
  SourceMissing,
  SourceEnvironment,
  SourceNpcCount,
  SourceNotNearPlayer
}

public sealed record PylonTeleportResult(
  bool Succeeded,
  PylonTeleportFailure Failure,
  TeleportPylonSnapshot? Source,
  TeleportPylonSnapshot? Target,
  int SourceNpcCount,
  int TargetNpcCount)
{
  public static PylonTeleportResult Rejected(
    PylonTeleportFailure failure,
    TeleportPylonSnapshot? target = null,
    int sourceNpcCount = 0,
    int targetNpcCount = 0)
  {
    return new PylonTeleportResult(
      false,
      failure,
      null,
      target,
      sourceNpcCount,
      targetNpcCount);
  }
}

/// <summary>Version4 source/target eligibility over one authoritative world capture.</summary>
public sealed class PylonInteractionService
{
  private readonly MechanismFactsCatalog _facts;

  public PylonInteractionService(MechanismFactsCatalog facts)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
  }

  public PylonTeleportResult Evaluate(PylonInteractionInput input)
  {
    ArgumentNullException.ThrowIfNull(input);
    return Evaluate(
      input.Player,
      input.TargetType,
      input.TargetAnchor,
      input.Pylons,
      input.Npcs,
      input.Environments,
      input.PlayerNearActivePylonTile);
  }

  public PylonTeleportResult Evaluate(
    PylonPlayerFact player,
    byte targetType,
    TileCoordinate targetAnchor,
    IReadOnlyList<TeleportPylonSnapshot> pylons,
    IReadOnlyList<PylonNpcFact> npcs,
    IReadOnlyDictionary<TileCoordinate, PylonEnvironmentFacts> environments)
  {
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(pylons);
    return Evaluate(player, targetType, targetAnchor, pylons, npcs, environments,
      pylons.Any(pylon => TeleportPylonRules.IsPlayerNearPylon(player, pylon.Anchor)));
  }

  /// <param name="playerNearActivePylonTile">
  /// P03's active tile-597 scan over P02's authoritative interaction rectangle.
  /// All inputs must be captured in the same exclusive owner window.
  /// </param>
  public PylonTeleportResult Evaluate(
    PylonPlayerFact player,
    byte targetType,
    TileCoordinate targetAnchor,
    IReadOnlyList<TeleportPylonSnapshot> pylons,
    IReadOnlyList<PylonNpcFact> npcs,
    IReadOnlyDictionary<TileCoordinate, PylonEnvironmentFacts> environments,
    bool playerNearActivePylonTile)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(environments);
    if (!_facts.Pylons.TryGetValue(targetType, out TeleportPylonDefinition? definition))
    {
      return PylonTeleportResult.Rejected(PylonTeleportFailure.TargetMissing);
    }

    TeleportPylonSnapshot? target = pylons.FirstOrDefault(pylon =>
      pylon.Type == targetType && pylon.Anchor == targetAnchor);
    if (target is null)
    {
      return PylonTeleportResult.Rejected(PylonTeleportFailure.TargetMissing);
    }

    if (!playerNearActivePylonTile)
    {
      return PylonTeleportResult.Rejected(PylonTeleportFailure.SourceNotNearPlayer, target);
    }

    int targetNpcCount = CountEligibleNpcs(target.Anchor, npcs);
    int requiredNpcCount = definition.RequiresTownNpc ? 2 : 0;
    if (targetNpcCount < requiredNpcCount)
    {
      return PylonTeleportResult.Rejected(
        PylonTeleportFailure.TargetNpcCount, target, targetNpcCount: targetNpcCount);
    }

    if (!environments.TryGetValue(target.Anchor, out PylonEnvironmentFacts? environment))
    {
      return PylonTeleportResult.Rejected(
        PylonTeleportFailure.TargetEnvironment, target, targetNpcCount: targetNpcCount);
    }

    if (environment.TempleWall && target.Anchor.Y > environment.WorldSurface &&
        !environment.DownedPlantBoss)
    {
      return PylonTeleportResult.Rejected(
        PylonTeleportFailure.AccessingTempleEarly,
        target: target,
        targetNpcCount: targetNpcCount);
    }

    if (!environment.IsValid(target.Type, target.Anchor))
    {
      return PylonTeleportResult.Rejected(
        PylonTeleportFailure.TargetEnvironment, target,
        targetNpcCount: targetNpcCount);
    }

    bool hasNearSource = false;
    bool hasNpcQualifiedSource = false;
    int greatestSourceNpcCount = 0;
    foreach (TeleportPylonSnapshot source in pylons.OrderBy(pylon => pylon.Ordinal))
    {
      if (!TeleportPylonRules.IsPlayerNearPylon(player, source.Anchor) ||
          !_facts.Pylons.TryGetValue(source.Type, out TeleportPylonDefinition? sourceDefinition))
      {
        continue;
      }

      hasNearSource = true;
      int sourceNpcCount = CountEligibleNpcs(source.Anchor, npcs);
      greatestSourceNpcCount = Math.Max(greatestSourceNpcCount, sourceNpcCount);
      if (sourceNpcCount < (sourceDefinition.RequiresTownNpc ? 2 : 0))
      {
        continue;
      }

      hasNpcQualifiedSource = true;
      if (!environments.TryGetValue(source.Anchor, out PylonEnvironmentFacts? sourceEnvironment) ||
          !sourceEnvironment.IsValid(source.Type, source.Anchor))
      {
        continue;
      }

      return new PylonTeleportResult(
        true,
        PylonTeleportFailure.None,
        source,
        target,
        sourceNpcCount,
        targetNpcCount);
    }

    return PylonTeleportResult.Rejected(
      !hasNearSource
        ? PylonTeleportFailure.SourceNotNearPlayer
        : hasNpcQualifiedSource
          ? PylonTeleportFailure.SourceEnvironment
          : PylonTeleportFailure.SourceNpcCount,
      target,
      greatestSourceNpcCount,
      targetNpcCount: targetNpcCount);
  }

  private static int CountEligibleNpcs(
    TileCoordinate pylon,
    IReadOnlyList<PylonNpcFact> npcs)
  {
    return npcs.Count(npc => npc.Active && npc.IsLikeTownNpc && !npc.Homeless &&
      TeleportPylonRules.IsNpcHomeInScene(npc.Home, pylon) &&
      TeleportPylonRules.IsNpcCenterNearHome(npc.CenterTileX, npc.CenterTileY, npc.Home));
  }
}
