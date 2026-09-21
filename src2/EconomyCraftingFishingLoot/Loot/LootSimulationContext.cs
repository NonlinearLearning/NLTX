namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class LootSimulationContext
{
  public LootSimulationContext(
    string? playerKey,
    double originalDayTimeCounter,
    bool originalDayTimeFlag,
    float originalPlayerPositionX,
    float originalPlayerPositionY,
    bool runningExpertMode,
    LootSimulationCounterProjection itemCounter,
    string? npcVictimKey)
  {
    if (playerKey is not null && string.IsNullOrWhiteSpace(playerKey))
    {
      throw new ArgumentException(
        "A player key must contain non-whitespace characters.",
        nameof(playerKey));
    }

    if (!double.IsFinite(originalDayTimeCounter))
    {
      throw new ArgumentOutOfRangeException(nameof(originalDayTimeCounter));
    }

    if (!float.IsFinite(originalPlayerPositionX))
    {
      throw new ArgumentOutOfRangeException(nameof(originalPlayerPositionX));
    }

    if (!float.IsFinite(originalPlayerPositionY))
    {
      throw new ArgumentOutOfRangeException(nameof(originalPlayerPositionY));
    }

    if (npcVictimKey is not null && string.IsNullOrWhiteSpace(npcVictimKey))
    {
      throw new ArgumentException(
        "An NPC victim key must contain non-whitespace characters.",
        nameof(npcVictimKey));
    }

    ArgumentNullException.ThrowIfNull(itemCounter);

    PlayerKey = playerKey;
    OriginalDayTimeCounter = originalDayTimeCounter;
    OriginalDayTimeFlag = originalDayTimeFlag;
    OriginalPlayerPositionX = originalPlayerPositionX;
    OriginalPlayerPositionY = originalPlayerPositionY;
    RunningExpertMode = runningExpertMode;
    ItemCounter = itemCounter;
    NpcVictimKey = npcVictimKey;
  }

  public string? PlayerKey { get; }

  public double OriginalDayTimeCounter { get; }

  public bool OriginalDayTimeFlag { get; }

  public float OriginalPlayerPositionX { get; }

  public float OriginalPlayerPositionY { get; }

  public bool RunningExpertMode { get; }

  public LootSimulationCounterProjection ItemCounter { get; }

  public string? NpcVictimKey { get; }
}
