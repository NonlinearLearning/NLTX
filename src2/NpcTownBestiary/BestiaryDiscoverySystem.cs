namespace Terraria.NpcTownBestiary;

public sealed class BestiaryDiscoverySystem
{
  private readonly BestiaryDiscoveryMutationGateAdapter _mutationGate;

  public BestiaryDiscoverySystem()
    : this(new BestiaryDiscoveryMutationGateAdapter())
  {
  }

  public BestiaryDiscoverySystem(BestiaryDiscoveryMutationGateAdapter mutationGate)
  {
    _mutationGate = mutationGate ?? throw new ArgumentNullException(nameof(mutationGate));
  }

  public BestiaryDiscoveryMutationResult RegisterKill(
    BestiaryKillCountStateComponent state,
    BestiaryCreditId creditId,
    int amount)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (amount <= 0)
    {
      return new BestiaryDiscoveryMutationResult(false, false, state.GetCount(creditId));
    }

    return _mutationGate.Execute(() =>
    {
      int before = state.GetCount(creditId);
      int after = state.Add(creditId, amount);
      return new BestiaryDiscoveryMutationResult(true, before != after, after);
    });
  }

  public BestiaryDiscoveryMutationResult RegisterSight(
    BestiarySightDiscoveryStateComponent state,
    BestiaryCreditId creditId)
  {
    ArgumentNullException.ThrowIfNull(state);
    return _mutationGate.Execute(() =>
    {
      bool changed = state.Add(creditId);
      return new BestiaryDiscoveryMutationResult(true, changed, changed ? 1 : 0);
    });
  }

  public BestiaryDiscoveryMutationResult SetKillCount(
    BestiaryKillCountStateComponent state,
    BestiaryCreditId creditId,
    int count)
  {
    ArgumentNullException.ThrowIfNull(state);
    return _mutationGate.Execute(() =>
    {
      bool changed = state.SetDirect(creditId, count);
      return new BestiaryDiscoveryMutationResult(true, changed, state.GetCount(creditId));
    });
  }

  public BestiaryDiscoveryMutationResult RegisterChat(
    BestiaryChatDiscoveryStateComponent state,
    BestiaryCreditId creditId)
  {
    ArgumentNullException.ThrowIfNull(state);
    return _mutationGate.Execute(() =>
    {
      bool changed = state.Add(creditId);
      return new BestiaryDiscoveryMutationResult(true, changed, changed ? 1 : 0);
    });
  }
}
