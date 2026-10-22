using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>Resolves validated P02 player movement into world-tile coordinates.</summary>
public sealed class PlayerOwnerQuickStackPositionProvider : IQuickStackPlayerPositionProvider
{
  private readonly PlayerOwner _players;

  public PlayerOwnerQuickStackPositionProvider(PlayerOwner players)
  {
    ArgumentNullException.ThrowIfNull(players);
    _players = players;
  }

  public bool TryGetPosition(
    EntityUuid authenticatedPlayerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    out QuickStackPlayerPosition position)
  {
    position = default;
    if (!authenticatedPlayerId.IsValid
      || !epoch.IsValid
      || !worldScope.IsValid
      || !_players.TryGet(epoch, out MinimalImplementation.Server.Player.Player? player)
      || player is null
      || player.Handle.Id != authenticatedPlayerId
      || player.Handle.WorldScope != worldScope)
    {
      return false;
    }

    const double pixelsPerWorldTile = 16.0;
    var pixels = player.Movement.Position;
    position = new QuickStackPlayerPosition(
      pixels.X / pixelsPerWorldTile,
      pixels.Y / pixelsPerWorldTile);
    return position.IsValid;
  }
}
