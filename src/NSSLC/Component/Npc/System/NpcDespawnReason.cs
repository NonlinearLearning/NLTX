namespace Terraria.Npc;

public enum NpcDespawnReason : byte
{
  None,
  TimeExpired,
  PopulationPressure,
  WorldUnload,
  DefinitionInvalid,
  ParentDestroyed,
  CompatibilityRemoval,
}
