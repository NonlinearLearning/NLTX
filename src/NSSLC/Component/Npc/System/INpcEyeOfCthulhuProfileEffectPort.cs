using System.Numerics;

namespace Terraria.Npc;

public interface INpcEyeOfCthulhuProfileEffectPort
{
  void SpawnDust(Vector2 position, int width, int height, Vector2 velocity);

  bool TrySpawnServant(Vector2 position, Vector2 velocity);

  void PlaySound(int soundId, Vector2 position);

  void SpawnGore(int goreId, Vector2 position, Vector2 velocity);

  void SetReflectsProjectiles(bool reflectsProjectiles);

  void BeginDash(Vector2 velocity);

  void EncourageDespawn(int ticks);

  void RequestNetworkUpdate();
}
