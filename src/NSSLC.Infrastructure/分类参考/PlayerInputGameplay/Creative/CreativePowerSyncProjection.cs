namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativePowerSyncProjection
{
  private readonly ICreativePowerNetworkAdapter _network;

  public CreativePowerSyncProjection(ICreativePowerNetworkAdapter network)
  {
    _network = network ?? throw new ArgumentNullException(nameof(network));
  }

  public void PublishShared(
    SharedCreativePowerDefinition definition,
    SharedCreativePowerStateComponent state,
    long sequence)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(state);
    if (!definition.SyncToJoiningPlayers)
    {
      return;
    }

    _network.Publish(new CreativePowerNetworkMessage(
      definition.PowerId,
      null,
      state.SliderCurrentValueCache,
      state.Enabled,
      sequence));
  }

  public void PublishPerPlayer(
    PerPlayerCreativePowerDefinition definition,
    PerPlayerCreativePowerStateComponent state,
    int playerSlot,
    long sequence)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(state);
    _network.Publish(new CreativePowerNetworkMessage(
      definition.PowerId,
      playerSlot,
      state.GetSlider(playerSlot),
      state.IsEnabled(playerSlot),
      sequence));
  }
}
