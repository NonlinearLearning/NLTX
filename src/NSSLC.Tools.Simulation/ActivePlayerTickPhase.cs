using Terraria.NonAuthoritative.Simulation;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActivePlayerTickPhase : IWorldSimulationTickPhase
{
  private readonly RuntimePlayerStore _players;
  private readonly RuntimeNpcStore _npcs;
  private readonly RuntimeProjectileStore _projectiles;
  private readonly ActivePressurePlateTickPhase _pressurePlateInteractions;

  public int PressurePlateActivationCount => _pressurePlateInteractions.ActivationCount;

  public int ActuatorToggleCount => _pressurePlateInteractions.ActuatorToggleCount;

  public IReadOnlyList<ushort> RecognizedUnsupportedWiredDeviceTileTypes =>
    _pressurePlateInteractions.RecognizedUnsupportedWiredDeviceTileTypes;

  public ActivePlayerTickPhase(
    RuntimePlayerStore players,
    RuntimeNpcStore npcs,
    RuntimeProjectileStore projectiles)
  {
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
    _projectiles = projectiles ?? throw new ArgumentNullException(nameof(projectiles));
    _pressurePlateInteractions = new ActivePressurePlateTickPhase(_players);
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.Player;

  public void Execute(WorldSimulationTickContext context)
  {
    _players.Update(context.TickNumber);
    _pressurePlateInteractions.Execute(context);
    _players.UseScriptedWeapons(context.TickNumber, _npcs, _projectiles);
  }
}
