namespace Terraria.WorldStorage;

/// <summary>Persistent identity and housing of an NPC in a world session.</summary>
public sealed class WorldNpcState : WorldEntityState {
  public int? NetId { get; }
  public string? LegacyTypeName { get; }
  public bool IsTownNpc { get; }
  public string Name { get; }
  public float X { get; }
  public float Y { get; }
  public bool Homeless { get; }
  public TileCoordinate Home { get; }
  public int? Variation { get; }
  public bool HomelessDespawn { get; }

  public WorldNpcState(int? netId, string? legacyTypeName, bool isTownNpc, string name,
      float x, float y, bool homeless, TileCoordinate home, int? variation,
      bool homelessDespawn) {
    NetId = netId;
    LegacyTypeName = legacyTypeName;
    IsTownNpc = isTownNpc;
    Name = name;
    X = x;
    Y = y;
    Homeless = homeless;
    Home = home;
    Variation = variation;
    HomelessDespawn = homelessDespawn;
  }
}
