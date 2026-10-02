namespace Terraria.Content;

public sealed record NpcCapabilitiesDefinition(bool Friendly, bool Hostile, bool IsBoss = false)
{
  public bool IsImmuneToLava { get; init; }

  public bool IsImmuneToWater { get; init; }

  public bool IsDungeon { get; init; }
}
