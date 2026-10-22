namespace MinimalImplementation.Combat.Npcs;

/// <summary>NPC-owned housing state. Minus one is the source's unset tile sentinel.</summary>
public sealed record NpcHomeState(bool Homeless, int HomeTileX, int HomeTileY) {
  // NPC.SetDefaults resets these independently; unset does not imply Homeless.
  internal static NpcHomeState ResetState { get; } = new(false, -1, -1);

  internal bool HasValidCoordinates => HomeTileX >= -1 && HomeTileY >= -1;
}
