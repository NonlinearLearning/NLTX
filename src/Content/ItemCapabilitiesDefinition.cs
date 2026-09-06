namespace Terraria.Content;

public sealed record ItemCapabilitiesDefinition(
  bool IsMount,
  bool IsSentry,
  bool IsVanity,
  int? MountTypeId = null,
  bool CartTrack = false,
  bool IsChlorophyteExtractinatorConsumable = false,
  bool IsDd2Summon = false,
  bool IsNewAndShiny = false);
