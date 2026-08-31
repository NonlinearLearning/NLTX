using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct SpawnNpcCommand(
  int DefinitionId,
  SimulationVector Position,
  NpcSpawnSource Source,
  float DifficultyScale = 1.0f,
  int ReleaseOwner = 0,
  int ReleaseVariant = 0,
  int RequestedReplicationId = 0,
  string? GivenName = null,
  bool CanBeReplaced = false,
  bool DoesNotCountMe = false,
  bool DoesNotTakeDamageFromHostiles = false,
  bool HomelessDespawn = false);
