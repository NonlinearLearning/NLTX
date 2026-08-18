using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation;

public readonly record struct ProjectileReplicationSnapshot(
  int ReplicationId,
  int ProjectileType,
  PlayerHandle Owner,
  SimulationVector Position,
  SimulationVector Velocity,
  int Damage,
  int RemainingLifetime,
  bool IsActive,
  long Revision,
  WorldSectionCoordinates Section,
  int Identity = 0,
  Guid? ProjectileUuid = null,
  float Ai0 = 0.0f,
  float Ai1 = 0.0f,
  float Ai2 = 0.0f,
  ushort Banner = 0,
  float Knockback = 0.0f,
  int OriginalDamage = 0,
  short Uuid = 0);
