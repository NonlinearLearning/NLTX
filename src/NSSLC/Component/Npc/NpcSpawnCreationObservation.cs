namespace Terraria.Npc;

/// <summary>
/// Describes how much the natural-spawn coordinator can observe about creation.
/// The current continuation port exposes only legacy loop control, so an accepted
/// candidate cannot be treated as proof that an NPC entity was created.
/// </summary>
public enum NpcSpawnCreationObservation : byte
{
  NotRequested,
  Unknown,
}
