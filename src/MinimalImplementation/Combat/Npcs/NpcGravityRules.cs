namespace MinimalImplementation.Combat.Npcs;

internal static class NpcGravityRules {
  // UpdateNPC_UpdateGravity's ordinary type branch, v1.4.5.6/319.
  internal static (float Gravity, float MaximumFallSpeed) Calculate(
      float positionY, double worldSurfaceTiles, int maximumTilesX,
      bool wasWet, bool wasHoneyWet, bool wasShimmerWet) {
    float worldSize = maximumTilesX / 4200f;
    worldSize *= worldSize;
    float factor = (float)((positionY / 16f - (60f + 10f * worldSize)) /
        (worldSurfaceTiles / 6d));
    float gravity = 0.3f * Math.Clamp(factor, 0.25f, 1f);
    if (!wasWet) {
      return (gravity, 10f);
    }
    if (wasShimmerWet) {
      return (0.15f, 5.5f);
    }
    return wasHoneyWet ? (0.1f, 4f) : (0.2f, 7f);
  }
}
