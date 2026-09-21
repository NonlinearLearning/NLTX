namespace NLTX.PlayerInputGameplay.Interaction;

public sealed class FakeCursorItemQuery
{
  public CursorItemSnapshot Combine(CursorItemSnapshot cursorItem, CursorItemSnapshot previewItem)
  {
    if (cursorItem.Type < 0 || cursorItem.Prefix < 0 || cursorItem.Stack < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursorItem));
    }

    if (previewItem.Type < 0 || previewItem.Prefix < 0 || previewItem.Stack < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(previewItem));
    }

    var cursorStack = cursorItem.IsAir ? 0 : cursorItem.Stack;
    return previewItem with { Stack = checked(previewItem.Stack + cursorStack) };
  }
}
