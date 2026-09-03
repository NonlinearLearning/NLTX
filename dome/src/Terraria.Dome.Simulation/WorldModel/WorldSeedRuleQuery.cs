using System;

namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Reads server-owned secret-seed rules from immutable world metadata.</summary>
public static class WorldSeedRuleQuery
{
  public static bool IsDrunkWorld(WorldMetadata metadata) => Read(metadata, Require(metadata).IsDrunkWorld);

  public static bool IsEverythingWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsEverythingWorld);

  public static bool IsDontStarveWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsDontStarveWorld);

  public static bool IsNotTheBeesWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsNotTheBeesWorld);

  public static bool IsZenithWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsZenithWorld);

  public static bool IsTenthAnniversaryWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsTenthAnniversaryWorld);

  public static bool IsVampireWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsVampireWorld);

  public static bool IsInfectedWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsInfectedWorld);

  public static bool IsTeamBasedSpawnsWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsTeamBasedSpawnsWorld);

  public static bool IsDualDungeonsWorld(WorldMetadata metadata) =>
    Read(metadata, Require(metadata).IsDualDungeonsWorld);

  private static bool Read(WorldMetadata metadata, bool? value)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return value == true;
  }

  private static WorldMetadata Require(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return metadata;
  }
}
