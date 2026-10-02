namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryUnlockProgressReport(
  int EntriesTotal,
  float CompletionAmountTotal)
{
  public float CompletionPercent => EntriesTotal == 0
    ? 1f
    : CompletionAmountTotal / EntriesTotal;
}
