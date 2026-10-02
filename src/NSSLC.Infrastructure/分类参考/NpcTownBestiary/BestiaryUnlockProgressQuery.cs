namespace Terraria.NpcTownBestiary;

public static class BestiaryUnlockProgressQuery
{
  public static BestiaryUnlockProgressReport Calculate(
    IReadOnlyCollection<BestiaryEntryUnlockState> states)
  {
    ArgumentNullException.ThrowIfNull(states);
    float completion = 0f;
    foreach (BestiaryEntryUnlockState state in states)
    {
      completion += (int)state / (float)BestiaryEntryUnlockState.CanShowDropsWithDropRates;
    }

    return new BestiaryUnlockProgressReport(states.Count, completion);
  }
}
