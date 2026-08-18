using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public readonly record struct LegacyWorldDataContext(
  int Time,
  byte WorldFlags,
  byte MoonPhase,
  short Width,
  short Height,
  short SpawnX,
  short SpawnY,
  short WorldSurface,
  short RockLayer,
  int WorldId,
  string WorldName,
  byte GameMode,
  Guid UniqueId,
  ulong WorldGeneratorVersion,
  byte MoonType,
  LegacyWorldBackgroundState Background,
  LegacyWorldProgressionState Progression,
  LegacyOreTierState OreTiers,
  LegacySpawnPointSet ExtraSpawnPoints)
{
  public static LegacyWorldDataContext CreateDomeDefaults()
  {
    const short worldWidth = 4200;
    const short worldHeight = 1200;
    return new LegacyWorldDataContext(
      Time: 0,
      WorldFlags: 0,
      MoonPhase: 0,
      Width: worldWidth,
      Height: worldHeight,
      SpawnX: worldWidth / 2,
      SpawnY: 300,
      WorldSurface: 300,
      RockLayer: 600,
      WorldId: 1,
      WorldName: "Dome World",
      GameMode: 0,
      UniqueId: Guid.Empty,
      WorldGeneratorVersion: 0,
      MoonType: 0,
      Background: default,
      Progression: default,
      OreTiers: default,
      ExtraSpawnPoints: new LegacySpawnPointSet([]));
  }

  public static LegacyWorldDataContext FromWorldMetadata(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    if (metadata.Width > short.MaxValue || metadata.Height > short.MaxValue ||
        metadata.SpawnX > short.MaxValue || metadata.SpawnY > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(metadata));
    }

    short worldSurface = (short)Math.Clamp(metadata.Height / 4, 0, short.MaxValue);
    short rockLayer = (short)Math.Clamp(metadata.Height / 2, 0, short.MaxValue);
    return new LegacyWorldDataContext(
      Time: 0,
      WorldFlags: 0,
      MoonPhase: 0,
      Width: (short)metadata.Width,
      Height: (short)metadata.Height,
      SpawnX: (short)metadata.SpawnX,
      SpawnY: (short)metadata.SpawnY,
      WorldSurface: worldSurface,
      RockLayer: rockLayer,
      WorldId: metadata.WorldId,
      WorldName: metadata.Name,
      GameMode: 0,
      UniqueId: Guid.Empty,
      WorldGeneratorVersion: 0,
      MoonType: 0,
      Background: default,
      Progression: default,
      OreTiers: default,
      ExtraSpawnPoints: new LegacySpawnPointSet([]));
  }

  public LegacyWorldDataContext WithWorldState(
    int time,
    bool isDayTime,
    WorldProgressionState progression,
    WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(rules);
    byte worldFlags = isDayTime ? (byte)1 : (byte)0;
    if (progression.IsBloodMoon)
    {
      worldFlags |= 2;
    }

    if (progression.IsEclipse)
    {
      worldFlags |= 4;
    }

    if (progression.InvasionType > sbyte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(progression));
    }

    LegacyWorldProgressionState legacyProgression = Progression with
    {
      EventFlags3 = progression.IsSlimeRaining
        ? (byte)(Progression.EventFlags3 | 4)
        : Progression.EventFlags3,
      InvasionType = (sbyte)progression.InvasionType
    };
    LegacyWorldBackgroundState background = Background with
    {
      MaximumRaining = rules.IsRaining ? rules.RainStrength : 0.0f
    };
    return this with
    {
      Time = time,
      WorldFlags = worldFlags,
      Background = background,
      Progression = legacyProgression
    };
  }
}
