using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class LootSimulationCounterProjection
{
  public LootSimulationCounterProjection(
    IEnumerable<long> obtainedCounts,
    IEnumerable<long> obtainedExpertCounts)
  {
    ArgumentNullException.ThrowIfNull(obtainedCounts);
    ArgumentNullException.ThrowIfNull(obtainedExpertCounts);

    ImmutableArray<long> normalSnapshot = obtainedCounts.ToImmutableArray();
    ImmutableArray<long> expertSnapshot = obtainedExpertCounts.ToImmutableArray();
    if (normalSnapshot.Length != expertSnapshot.Length)
    {
      throw new ArgumentException(
        "Normal and expert loot counters must have the same item count.",
        nameof(obtainedExpertCounts));
    }

    if (normalSnapshot.Any(count => count < 0) ||
        expertSnapshot.Any(count => count < 0))
    {
      throw new ArgumentException(
        "Loot simulation counters cannot contain negative counts.",
        nameof(obtainedCounts));
    }

    ObtainedCounts = normalSnapshot;
    ObtainedExpertCounts = expertSnapshot;
  }

  private LootSimulationCounterProjection(
    ImmutableArray<long> obtainedCounts,
    ImmutableArray<long> obtainedExpertCounts,
    bool alreadyValidated)
  {
    _ = alreadyValidated;
    ObtainedCounts = obtainedCounts;
    ObtainedExpertCounts = obtainedExpertCounts;
  }

  public ImmutableArray<long> ObtainedCounts { get; }

  public ImmutableArray<long> ObtainedExpertCounts { get; }

  public int ItemTypeCount => ObtainedCounts.Length;

  public long GetObtainedCount(int itemTypeId, bool expertMode)
  {
    ValidateItemTypeId(itemTypeId);
    return expertMode
      ? ObtainedExpertCounts[itemTypeId]
      : ObtainedCounts[itemTypeId];
  }

  public LootSimulationCounterProjection WithObtained(
    int itemTypeId,
    bool expertMode,
    long amount)
  {
    ValidateItemTypeId(itemTypeId);
    if (amount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    ImmutableArray<long>.Builder normalBuilder = ObtainedCounts.ToBuilder();
    ImmutableArray<long>.Builder expertBuilder = ObtainedExpertCounts.ToBuilder();
    if (expertMode)
    {
      expertBuilder[itemTypeId] = checked(expertBuilder[itemTypeId] + amount);
    }
    else
    {
      normalBuilder[itemTypeId] = checked(normalBuilder[itemTypeId] + amount);
    }

    return new LootSimulationCounterProjection(
      normalBuilder.MoveToImmutable(),
      expertBuilder.MoveToImmutable(),
      alreadyValidated: true);
  }

  private void ValidateItemTypeId(int itemTypeId)
  {
    if ((uint)itemTypeId >= (uint)ItemTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeId));
    }
  }
}
