using System.Numerics;

namespace Terraria.Npc;

// Next and NewDust must use the same random stream to preserve legacy draw ordering.
public interface INpcSoulDrainVisualEffectPort
{
  int Next(int minimumInclusive, int maximumExclusive);

  int NewDust(Vector2 position, int width, int height, int dustType);

  void SetDustVelocity(int dustIndex, Vector2 velocity);

  void SetDustScale(int dustIndex, float scale);

  void SetDustFadeIn(int dustIndex, float fadeIn);
}
