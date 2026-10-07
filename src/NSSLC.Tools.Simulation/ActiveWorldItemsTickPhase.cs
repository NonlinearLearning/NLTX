using Terraria.NonAuthoritative.Simulation;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActiveWorldItemsTickPhase : IWorldSimulationTickPhase
{
  private readonly RuntimeWorldItemStore _items;
  private readonly RuntimePlayerStore _players;

  public ActiveWorldItemsTickPhase(RuntimeWorldItemStore items, RuntimePlayerStore players)
  {
    _items = items ?? throw new ArgumentNullException(nameof(items));
    _players = players ?? throw new ArgumentNullException(nameof(players));
  }

  public WorldSimulationPhase Phase => WorldSimulationPhase.WorldItems;

  public void Execute(WorldSimulationTickContext context)
  {
    _items.Update(context.TickNumber, _players);
  }
}
