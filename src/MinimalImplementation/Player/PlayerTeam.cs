namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerTeam(byte Previous, byte Current)
{
  public const byte MaximumTeam = 5;

  public bool TryChange(byte requestedTeam, out PlayerTeam updated)
  {
    if (requestedTeam > MaximumTeam)
    {
      updated = default;
      return false;
    }

    updated = new PlayerTeam(this.Current, requestedTeam);
    return true;
  }
}
