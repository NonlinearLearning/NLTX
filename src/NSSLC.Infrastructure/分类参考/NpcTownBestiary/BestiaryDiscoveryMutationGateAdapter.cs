namespace Terraria.NpcTownBestiary;

public sealed class BestiaryDiscoveryMutationGateAdapter
{
  private readonly object _gate = new();

  public T Execute<T>(Func<T> mutation)
  {
    ArgumentNullException.ThrowIfNull(mutation);
    lock (_gate)
    {
      return mutation();
    }
  }
}
