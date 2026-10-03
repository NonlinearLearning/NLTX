using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The environment prefix that follows the identity and rule flags in a pointer-based WorldFile
/// header. It is a persistence DTO; it does not reference the runtime world or an ECS store.
/// </summary>
public sealed class WorldFileEnvironmentSection
{
  public const string SectionId = "world.environment";

  public WorldFileEnvironmentSection(
    byte moonType,
    IReadOnlyList<int> treeX,
    IReadOnlyList<int> treeStyle,
    IReadOnlyList<int> caveBackX,
    IReadOnlyList<int> caveBackStyle,
    int iceBackStyle,
    int jungleBackStyle,
    int hellBackStyle,
    int spawnTileX,
    int spawnTileY,
    double worldSurface,
    double rockLayer,
    double time,
    bool dayTime,
    int moonPhase,
    bool bloodMoon,
    bool eclipse,
    int dungeonX,
    int dungeonY,
    bool crimson)
  {
    TreeX = CopyFixed(treeX, 3, nameof(treeX));
    TreeStyle = CopyFixed(treeStyle, 4, nameof(treeStyle));
    CaveBackX = CopyFixed(caveBackX, 3, nameof(caveBackX));
    CaveBackStyle = CopyFixed(caveBackStyle, 4, nameof(caveBackStyle));
    if (!double.IsFinite(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    if (!double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayer));
    }

    if (!double.IsFinite(time))
    {
      throw new ArgumentOutOfRangeException(nameof(time));
    }

    MoonType = moonType;
    IceBackStyle = iceBackStyle;
    JungleBackStyle = jungleBackStyle;
    HellBackStyle = hellBackStyle;
    SpawnTileX = spawnTileX;
    SpawnTileY = spawnTileY;
    WorldSurface = worldSurface;
    RockLayer = rockLayer;
    Time = time;
    DayTime = dayTime;
    MoonPhase = moonPhase;
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    DungeonX = dungeonX;
    DungeonY = dungeonY;
    Crimson = crimson;
  }

  public byte MoonType { get; }

  public IReadOnlyList<int> TreeX { get; }

  public IReadOnlyList<int> TreeStyle { get; }

  public IReadOnlyList<int> CaveBackX { get; }

  public IReadOnlyList<int> CaveBackStyle { get; }

  public int IceBackStyle { get; }

  public int JungleBackStyle { get; }

  public int HellBackStyle { get; }

  public int SpawnTileX { get; }

  public int SpawnTileY { get; }

  public double WorldSurface { get; }

  public double RockLayer { get; }

  public double Time { get; }

  public bool DayTime { get; }

  public int MoonPhase { get; }

  public bool BloodMoon { get; }

  public bool Eclipse { get; }

  public int DungeonX { get; }

  public int DungeonY { get; }

  public bool Crimson { get; }

  public static WorldFileEnvironmentSection Empty => new(
    moonType: 0,
    treeX: new int[3],
    treeStyle: new int[4],
    caveBackX: new int[3],
    caveBackStyle: new int[4],
    iceBackStyle: 0,
    jungleBackStyle: 0,
    hellBackStyle: 0,
    spawnTileX: 0,
    spawnTileY: 0,
    worldSurface: 0,
    rockLayer: 0,
    time: 0,
    dayTime: true,
    moonPhase: 0,
    bloodMoon: false,
    eclipse: false,
    dungeonX: 0,
    dungeonY: 0,
    crimson: false);

  private static IReadOnlyList<int> CopyFixed(
    IReadOnlyList<int> values,
    int expectedCount,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values);
    if (values.Count != expectedCount)
    {
      throw new ArgumentException(
        $"The environment section requires exactly {expectedCount} values.",
        parameterName);
    }

    return Array.AsReadOnly(new List<int>(values).ToArray());
  }
}
