using System.Numerics;

namespace Terraria.Npc;

public interface INpcFloatingEyeProfileEffectPort
{
  void EncourageDespawn(int ticks);

  void RequestTargetReacquire();

  void SpawnDust(Vector2 position, int width, int height, Vector2 velocity);
}
