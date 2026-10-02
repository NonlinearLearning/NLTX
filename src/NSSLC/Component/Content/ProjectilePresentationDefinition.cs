namespace Terraria.Content;

public sealed record ProjectilePresentationDefinition(
  int FrameCount,
  float Light,
  bool Hide)
{
  public int Alpha { get; init; }

  public int TrailCacheLength { get; init; } = 10;

  public short? GlowMaskId { get; init; }

  public int DrawLayer { get; init; }
}
