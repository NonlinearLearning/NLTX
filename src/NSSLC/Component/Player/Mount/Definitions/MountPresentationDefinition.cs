using System.Numerics;

namespace Terraria.Player.Mount;

public readonly record struct MountPresentationDefinition(
  Vector3 LightColor,
  bool EmitsLight,
  int SpawnDust,
  bool SpawnDustNoGravity);
