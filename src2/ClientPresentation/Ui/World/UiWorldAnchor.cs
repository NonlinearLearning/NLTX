using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.World;

public readonly record struct UiWorldAnchor(
  UiWorldAnchor.AnchorKind Kind,
  int EntitySlot,
  int TileX,
  int TileY,
  UiVector2 Position,
  UiVector2 Size = default)
{
  public static UiWorldAnchor None => new(
    AnchorKind.None,
    0,
    0,
    0,
    UiVector2.Zero);

  public bool IsValid => Kind switch
  {
    AnchorKind.None => IsValidSize,
    AnchorKind.Entity => EntitySlot > 0 && IsValidSize,
    AnchorKind.Tile => TileX >= 0 && TileY >= 0 && IsValidSize,
    AnchorKind.Position => float.IsFinite(Position.X)
      && float.IsFinite(Position.Y)
      && IsValidSize,
    _ => false
  };

  private bool IsValidSize => float.IsFinite(Size.X)
    && float.IsFinite(Size.Y)
    && Size.X >= 0f
    && Size.Y >= 0f;

  public static bool TryForEntity(int entitySlot, out UiWorldAnchor anchor)
  {
    anchor = new UiWorldAnchor(
      AnchorKind.Entity,
      entitySlot,
      0,
      0,
      UiVector2.Zero,
      UiVector2.Zero);
    return anchor.IsValid;
  }

  public static bool TryForTile(int tileX, int tileY, out UiWorldAnchor anchor)
  {
    anchor = new UiWorldAnchor(
      AnchorKind.Tile,
      0,
      tileX,
      tileY,
      UiVector2.Zero,
      new UiVector2(16f, 16f));
    return anchor.IsValid;
  }

  public static bool TryForPosition(UiVector2 position, out UiWorldAnchor anchor)
  {
    anchor = new UiWorldAnchor(
      AnchorKind.Position,
      0,
      0,
      0,
      position,
      UiVector2.Zero);
    return anchor.IsValid;
  }

  public enum AnchorKind
  {
    None,
    Entity,
    Tile,
    Position
  }
}
