using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropResolutionContext
{
  public DropResolutionContext(
    IDropRandomSource random,
    int npcNetId = 0,
    int npcTypeId = 0,
    string? npcName = null,
    IEnumerable<float>? npcAiValues = null,
    int wave = 0,
    bool isInSimulation = false,
    bool isExpertMode = false,
    bool isMasterMode = false,
    bool shouldDropExtraGel = false,
    string? playerKey = null)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (npcTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcTypeId));
    }

    if (wave < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wave));
    }

    if (playerKey is not null && string.IsNullOrWhiteSpace(playerKey))
    {
      throw new ArgumentException(
        "A player key must contain non-whitespace characters.",
        nameof(playerKey));
    }

    ImmutableArray<float> aiSnapshot = (npcAiValues ?? []).ToImmutableArray();
    if (aiSnapshot.Any(value => !float.IsFinite(value)))
    {
      throw new ArgumentException(
        "NPC AI values must be finite.",
        nameof(npcAiValues));
    }

    Random = random;
    NpcNetId = npcNetId;
    NpcTypeId = npcTypeId;
    NpcName = npcName ?? string.Empty;
    NpcAiValues = aiSnapshot;
    Wave = wave;
    IsInSimulation = isInSimulation;
    IsExpertMode = isExpertMode;
    IsMasterMode = isMasterMode;
    ShouldDropExtraGel = shouldDropExtraGel;
    PlayerKey = playerKey;
  }

  public IDropRandomSource Random { get; }

  public int NpcNetId { get; }

  public int NpcTypeId { get; }

  public string NpcName { get; }

  public ImmutableArray<float> NpcAiValues { get; }

  public int Wave { get; }

  public bool IsInSimulation { get; }

  public bool IsExpertMode { get; }

  public bool IsMasterMode { get; }

  public bool ShouldDropExtraGel { get; }

  public string? PlayerKey { get; }

  public bool TryGetNpcAiValue(int slot, out float value)
  {
    if ((uint)slot >= (uint)NpcAiValues.Length)
    {
      value = default;
      return false;
    }

    value = NpcAiValues[slot];
    return true;
  }
}
