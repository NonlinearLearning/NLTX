using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public readonly record struct LegacyNpcWireState(
  NpcReplicationSnapshot Snapshot,
  ushort Target,
  bool DirectionRight,
  bool DirectionYDown,
  float Ai0,
  float Ai1,
  float Ai2,
  float Ai3,
  bool SpriteDirectionRight,
  int LifeMaximum,
  byte PlayersForScaling,
  bool SpawnedFromStatue,
  float Difficulty,
  bool SpawnNeedsSyncing,
  float ShimmerTransparency,
  byte ReleaseOwner,
  bool IsCatchable)
{
  public static LegacyNpcWireState CreateDefault(NpcReplicationSnapshot snapshot)
  {
    int lifeMaximum = snapshot.Health == int.MaxValue ? int.MaxValue : snapshot.Health + 1;
    return new LegacyNpcWireState(
      Snapshot: snapshot,
      Target: 0,
      DirectionRight: false,
      DirectionYDown: false,
      Ai0: 0,
      Ai1: 0,
      Ai2: 0,
      Ai3: 0,
      SpriteDirectionRight: false,
      LifeMaximum: Math.Max(lifeMaximum, 1),
      PlayersForScaling: 1,
      SpawnedFromStatue: false,
      Difficulty: 1,
      SpawnNeedsSyncing: false,
      ShimmerTransparency: 0,
      ReleaseOwner: 0,
      IsCatchable: false);
  }
}
