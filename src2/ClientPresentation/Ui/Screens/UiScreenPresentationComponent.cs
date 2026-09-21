using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Screens;

public sealed class UiScreenPresentationComponent
{
  private readonly List<UiWorldSummarySnapshot> _favoritesCache = [];

  public UiScreenPresentationComponent(UiElementId hostId, ScreenKind kind)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    HostId = hostId;
    Kind = kind;
  }

  public UiElementId HostId { get; }

  public ScreenKind Kind { get; }

  public bool IsActive { get; private set; }

  public UiElementId? ProgressBarId { get; private set; }

  public UiElementId? ProgressMessageId { get; private set; }

  public UiElementId? WorldListId { get; private set; }

  public UiElementId? ContainerPanelId { get; private set; }

  public UiElementId? ScrollbarId { get; private set; }

  public bool IsScrollbarAttached { get; private set; }

  public UiWorldSummarySnapshot? NewlyGeneratedWorld { get; private set; }

  public IReadOnlyList<UiWorldSummarySnapshot> FavoritesCache
    => _favoritesCache.ToArray();

  internal void SetActive(bool isActive)
  {
    IsActive = isActive;
  }

  internal void SetChildren(
    UiElementId? progressBarId,
    UiElementId? progressMessageId,
    UiElementId? worldListId,
    UiElementId? containerPanelId,
    UiElementId? scrollbarId)
  {
    ProgressBarId = progressBarId;
    ProgressMessageId = progressMessageId;
    WorldListId = worldListId;
    ContainerPanelId = containerPanelId;
    ScrollbarId = scrollbarId;
    IsScrollbarAttached = false;
  }

  internal void SetScrollbarAttached(bool isAttached)
  {
    IsScrollbarAttached = isAttached && ScrollbarId.HasValue;
  }

  internal void SetWorldSummary(UiWorldSummarySnapshot? summary)
  {
    if (summary.HasValue && !summary.Value.IsValid)
    {
      throw new ArgumentException("The world summary is invalid.", nameof(summary));
    }

    NewlyGeneratedWorld = summary;
  }

  internal void SetFavoritesCache(IReadOnlyList<UiWorldSummarySnapshot> summaries)
  {
    ArgumentNullException.ThrowIfNull(summaries);
    if (summaries.Any(summary => !summary.IsValid))
    {
      throw new ArgumentException("The favorites snapshot is invalid.", nameof(summaries));
    }

    _favoritesCache.Clear();
    _favoritesCache.AddRange(summaries.Where(summary => summary.IsFavorite));
  }

  internal void ClearTransientState()
  {
    IsActive = false;
    ProgressBarId = null;
    ProgressMessageId = null;
    WorldListId = null;
    ContainerPanelId = null;
    ScrollbarId = null;
    IsScrollbarAttached = false;
    NewlyGeneratedWorld = null;
    _favoritesCache.Clear();
  }

  public enum ScreenKind
  {
    WorldLoad,
    WorldSelect
  }
}
