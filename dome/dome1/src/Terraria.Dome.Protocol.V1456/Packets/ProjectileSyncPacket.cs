using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ProjectileSyncPacket(
  short Identity,
  SimulationVector Position,
  SimulationVector Velocity,
  byte Owner,
  short ProjectileType,
  float? Ai0,
  float? Ai1,
  float? Ai2,
  ushort? Banner,
  short? Damage,
  float? Knockback,
  short? OriginalDamage,
  short? Uuid);
