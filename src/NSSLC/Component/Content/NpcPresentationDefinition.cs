namespace Terraria.Content;

public sealed record NpcPresentationDefinition(
  int FrameCount = 1,
  int? AltTextureId = null,
  string? BestiaryCreditId = null,
  int? BestiarySortingId = null,
  int? BestiaryRarityStars = null,
  bool BestiaryHidden = false,
  int TrailCacheLength = 0);
