namespace Terraria.WorldSession.Runtime;

public sealed class ClientIdentityAdapter
{
  public Guid? ClientId { get; private set; }

  public void Set(Guid clientId)
  {
    if (clientId == Guid.Empty)
    {
      throw new ArgumentException("A non-empty client ID is required.", nameof(clientId));
    }

    ClientId = clientId;
  }

  public void Clear()
  {
    ClientId = null;
  }
}
