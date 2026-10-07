namespace Terraria.WorldSession.Components;

/// <summary>
/// Performs the pure outbound saved-tier packet value mapping.
/// </summary>
public sealed class NetMessageSavedOreTierAdapter : ISavedOreTierNetworkProjection
{
  public WorldSavedOreTierNetworkFields Encode(in OreTierState state)
  {
    return new WorldSavedOreTierNetworkFields(
      unchecked((short)state.Copper),
      unchecked((short)state.Iron),
      unchecked((short)state.Silver),
      unchecked((short)state.Gold),
      unchecked((short)state.Cobalt),
      unchecked((short)state.Mythril),
      unchecked((short)state.Adamantite));
  }
}
