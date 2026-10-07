namespace Terraria.Npc;

/// <summary>
/// Describes how much the natural-spawn coordinator can observe about creation.
/// Legacy ports that expose only loop control report Unknown; observed ports can report
/// whether an NPC entity was allocated.
/// </summary>
public enum NpcSpawnCreationObservation : byte
{
  NotRequested,
  Unknown,
  Created,
  NotCreated,
}
