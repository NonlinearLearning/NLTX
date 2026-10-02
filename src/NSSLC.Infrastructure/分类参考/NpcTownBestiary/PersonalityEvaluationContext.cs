namespace Terraria.NpcTownBestiary;

public readonly record struct PersonalityEvaluationContext(
  NpcEntityId NpcEntityId,
  PlayerEntityId PlayerEntityId,
  IReadOnlyList<NpcReadView> NearbyNpcs,
  IReadOnlyCollection<string> ActiveBiomeKeys);
