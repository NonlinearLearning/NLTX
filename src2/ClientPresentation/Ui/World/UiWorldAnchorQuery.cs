using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.World;

public static class UiWorldAnchorQuery
{
  public static bool TryGetScreenPosition(
    UiWorldAnchor anchor,
    UiVector2 cameraPosition,
    IEntityPositionResolver entityResolver,
    out UiVector2 screenPosition)
  {
    ArgumentNullException.ThrowIfNull(entityResolver);
    switch (anchor.Kind)
    {
      case UiWorldAnchor.AnchorKind.Position:
        screenPosition = anchor.Position - cameraPosition;
        return anchor.IsValid;
      case UiWorldAnchor.AnchorKind.Tile:
        screenPosition = new UiVector2(
          anchor.TileX * 16f,
          anchor.TileY * 16f) - cameraPosition;
        return anchor.IsValid;
      case UiWorldAnchor.AnchorKind.Entity:
        if (entityResolver.TryGetPosition(anchor.EntitySlot, out UiVector2 position))
        {
          screenPosition = position - cameraPosition;
          return true;
        }

        break;
    }

    screenPosition = default;
    return false;
  }

  public interface IEntityPositionResolver
  {
    bool TryGetPosition(int entitySlot, out UiVector2 position);
  }
}
