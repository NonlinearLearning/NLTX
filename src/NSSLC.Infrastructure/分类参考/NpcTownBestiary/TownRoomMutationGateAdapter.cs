namespace Terraria.NpcTownBestiary;

// The Version4 EntityCreationLock is kept at the adapter boundary.
public sealed class TownRoomMutationGateAdapter
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
