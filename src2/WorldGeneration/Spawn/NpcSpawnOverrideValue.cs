namespace Terraria.WorldGeneration.Spawn;

public readonly record struct NpcSpawnOverrideValue(
  float? SizeScaleOverride = null,
  int? PlayerCountForMultiplayerDifficultyOverride = null,
  float? DifficultyOverride = null)
{
  public NpcSpawnOverrideValue WithScale(float scaleOverride)
  {
    if (!float.IsFinite(scaleOverride))
    {
      throw new ArgumentOutOfRangeException(nameof(scaleOverride));
    }

    return this with { SizeScaleOverride = scaleOverride };
  }
}
