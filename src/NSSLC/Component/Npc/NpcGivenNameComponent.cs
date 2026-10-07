namespace Terraria.Npc;

public sealed class NpcGivenNameComponent
{
  public NpcGivenNameComponent(string? givenName)
  {
    GivenName = givenName ?? string.Empty;
  }

  public string GivenName { get; private set; }

  public void SetGivenName(string? givenName)
  {
    GivenName = givenName ?? string.Empty;
  }
}
