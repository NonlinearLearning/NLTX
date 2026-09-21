using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Screens;

public sealed class UiScreenLifecycleSystem
{
  private readonly UiScreenPresentationComponent _screen;

  public UiScreenLifecycleSystem(UiScreenPresentationComponent screen)
  {
    _screen = screen ?? throw new ArgumentNullException(nameof(screen));
  }

  public bool AttachChildren(
    UiElementId? progressBarId,
    UiElementId? progressMessageId,
    UiElementId? worldListId,
    UiElementId? containerPanelId,
    UiElementId? scrollbarId)
  {
    if (!AreValid(progressBarId, progressMessageId, worldListId, containerPanelId, scrollbarId))
    {
      return false;
    }

    _screen.SetChildren(
      progressBarId,
      progressMessageId,
      worldListId,
      containerPanelId,
      scrollbarId);
    return true;
  }

  public bool Activate()
  {
    _screen.SetActive(true);
    return true;
  }

  public bool Deactivate()
  {
    _screen.ClearTransientState();
    return true;
  }

  public bool SetScrollbarAttachment(bool isAttached)
  {
    if (!_screen.IsActive || !_screen.ScrollbarId.HasValue)
    {
      return false;
    }

    _screen.SetScrollbarAttached(isAttached);
    return true;
  }

  public bool AcceptWorldSummary(UiWorldSummarySnapshot? summary)
  {
    try
    {
      _screen.SetWorldSummary(summary);
      return true;
    }
    catch (ArgumentException)
    {
      return false;
    }
  }

  public bool AcceptFavoritesSnapshot(IReadOnlyList<UiWorldSummarySnapshot> summaries)
  {
    try
    {
      _screen.SetFavoritesCache(summaries);
      return true;
    }
    catch (ArgumentException)
    {
      return false;
    }
  }

  private static bool AreValid(params UiElementId?[] ids)
  {
    return ids.All(id => !id.HasValue || id.Value.IsValid);
  }
}
