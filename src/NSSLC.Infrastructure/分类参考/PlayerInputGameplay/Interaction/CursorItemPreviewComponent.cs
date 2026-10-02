namespace NLTX.PlayerInputGameplay.Interaction;

public sealed class CursorItemPreviewComponent
{
  private CursorItemSnapshot _cursorItem = CursorItemSnapshot.Air;
  private CursorItemSnapshot _previewItem = CursorItemSnapshot.Air;

  public CursorItemSnapshot CursorItem => _cursorItem;

  public CursorItemSnapshot PreviewItem => _previewItem;

  public void SetCursorItem(CursorItemSnapshot item)
  {
    Validate(item);
    _cursorItem = item;
  }

  public void SetPreview(int type, int prefix, int stack)
  {
    var item = new CursorItemSnapshot(type, prefix, stack);
    Validate(item);
    _previewItem = item;
  }

  public CursorItemSnapshot GetCombinedPreview()
  {
    var stack = checked(_previewItem.Stack + (_cursorItem.IsAir ? 0 : _cursorItem.Stack));
    return _previewItem with { Stack = stack };
  }

  private static void Validate(CursorItemSnapshot item)
  {
    if (item.Type < 0 || item.Prefix < 0 || item.Stack < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(item));
    }
  }
}
