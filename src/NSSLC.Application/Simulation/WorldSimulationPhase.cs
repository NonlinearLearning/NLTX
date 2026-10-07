namespace Terraria.NonAuthoritative.Simulation;

public enum WorldSimulationPhase : byte
{
  CommandDrain = 0,
  WorldClock = 1,
  SnapshotCommit = 2,
  RuntimeProjection = 3,
  Player = 4,
  Npc = 5,
  Projectile = 6,
  WorldItems = 7,
  TileEntities = 8,
  WorldSystems = 9,
  TickCommit = 10,
}
