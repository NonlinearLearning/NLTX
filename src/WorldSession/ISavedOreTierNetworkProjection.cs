namespace Terraria.WorldSession.Components;

/// <summary>
/// Projects saved-tier values into the confirmed outbound packet field order.
/// </summary>
public interface ISavedOreTierNetworkProjection
{
  WorldSavedOreTierNetworkFields Encode(in OreTierState state);
}
