namespace Terraria.Player;

public sealed partial class PlayerCombatProcSystem
{
  private readonly HashSet<Guid> _acceptedEventIds = [];
  private readonly PlayerCombatProcStateComponent _component;

  public PlayerCombatProcSystem(PlayerCombatProcStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    _component = component;
  }
}
