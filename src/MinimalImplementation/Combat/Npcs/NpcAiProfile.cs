namespace MinimalImplementation.Combat.Npcs;

/// <summary>Typed behavior families selected by the Version4 NPC aiStyle dispatch.</summary>
public enum NpcAiProfileKind
{
  Unsupported,
  Passive,
  Slime,
  Floating,
  Fighter,
  Flying
}

public sealed record NpcAiProfile(
  int AiStyle,
  NpcAiProfileKind Kind,
  bool UsesTarget,
  bool UsesTileCollision,
  string Evidence)
{
  public bool IsSupported => Kind != NpcAiProfileKind.Unsupported;
}
