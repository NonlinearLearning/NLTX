namespace Terraria.Player.Progression;

public readonly record struct PlayerProgressionCommandToken(Guid Value)
{
  public bool IsValid => Value != Guid.Empty;
}
