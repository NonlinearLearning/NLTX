using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Collections;

public sealed class UiCollectionProgressComponent
{
  private readonly List<UiElementId> _items = [];

  public UiCollectionProgressComponent(
    UiElementId hostId,
    float listPadding = 0f,
    bool autoHide = true,
    uint borderColor = 0xffffffff,
    uint backgroundColor = 0xff000000)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    HostId = hostId;
    ListPadding = ValidateNonNegative(listPadding, nameof(listPadding));
    AutoHide = autoHide;
    BorderColor = borderColor;
    BackgroundColor = backgroundColor;
  }

  public UiElementId HostId { get; }

  public IReadOnlyList<UiElementId> Items => _items.ToArray();

  public float ListPadding { get; }

  public UiElementId? ScrollbarId { get; private set; }

  public bool AutoHide { get; }

  public uint BorderColor { get; }

  public uint BackgroundColor { get; }

  public float OverallProgressTarget { get; private set; }

  public float CurrentProgressTarget { get; private set; }

  public float RequestedScrollPosition { get; private set; }

  public float RequestedViewSize { get; private set; }

  public float ItemExtent { get; private set; }

  public float ContentExtent { get; private set; }

  public float ViewPosition { get; private set; }

  public float ViewSize { get; private set; }

  public float MaxViewSize { get; private set; }

  public bool CanScroll { get; private set; }

  public bool IsScrollbarVisible { get; private set; }

  public int FirstVisibleIndex { get; private set; }

  public int LastVisibleIndex { get; private set; } = -1;

  public bool Apply(UiCollectionCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);
    return command.Operation switch
    {
      UiCollectionCommand.OperationKind.Add => Add(command.ItemId),
      UiCollectionCommand.OperationKind.Remove => Remove(command.ItemId),
      UiCollectionCommand.OperationKind.Clear => Clear(),
      UiCollectionCommand.OperationKind.SortAscending => Sort(ascending: true),
      UiCollectionCommand.OperationKind.SortDescending => Sort(ascending: false),
      UiCollectionCommand.OperationKind.SetScroll => SetRequestedScroll(
        command.Value),
      UiCollectionCommand.OperationKind.SetView => SetRequestedView(
        command.Value),
      UiCollectionCommand.OperationKind.AttachScrollbar => AttachScrollbar(
        command.ScrollbarId),
      _ => false
    };
  }

  internal bool ApplyProgress(float overallProgress, float currentProgress)
  {
    if (!float.IsFinite(overallProgress)
      || !float.IsFinite(currentProgress)
      || overallProgress is < 0f or > 1f
      || currentProgress is < 0f or > 1f)
    {
      return false;
    }

    OverallProgressTarget = overallProgress;
    CurrentProgressTarget = currentProgress;
    return true;
  }

  internal void RecalculateLayout(float itemExtent, float viewSize)
  {
    if (!float.IsFinite(itemExtent) || itemExtent < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(itemExtent));
    }

    if (!float.IsFinite(viewSize) || viewSize < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(viewSize));
    }

    ItemExtent = itemExtent;
    ViewSize = viewSize;
    ContentExtent = ListPadding * 2f + _items.Count * itemExtent;
    MaxViewSize = ContentExtent;
    float maximumScroll = Math.Max(0f, MaxViewSize - ViewSize);
    ViewPosition = Math.Clamp(RequestedScrollPosition, 0f, maximumScroll);
    CanScroll = maximumScroll > 0f;
    IsScrollbarVisible = !AutoHide || CanScroll;
    if (itemExtent == 0f || _items.Count == 0)
    {
      FirstVisibleIndex = 0;
      LastVisibleIndex = _items.Count - 1;
      return;
    }

    FirstVisibleIndex = Math.Clamp(
      (int)Math.Floor(Math.Max(0f, ViewPosition - ListPadding) / itemExtent),
      0,
      _items.Count - 1);
    LastVisibleIndex = Math.Clamp(
      (int)Math.Ceiling(
        Math.Max(0f, ViewPosition + ViewSize - ListPadding) / itemExtent) - 1,
      FirstVisibleIndex,
      _items.Count - 1);
  }

  private bool Add(UiElementId itemId)
  {
    if (!itemId.IsValid || _items.Contains(itemId))
    {
      return false;
    }

    _items.Add(itemId);
    return true;
  }

  private bool Remove(UiElementId itemId)
  {
    return _items.Remove(itemId);
  }

  private bool Clear()
  {
    _items.Clear();
    return true;
  }

  private bool Sort(bool ascending)
  {
    _items.Sort((left, right) => ascending
      ? left.Value.CompareTo(right.Value)
      : right.Value.CompareTo(left.Value));
    return true;
  }

  private bool SetRequestedScroll(float value)
  {
    if (!float.IsFinite(value) || value < 0f)
    {
      return false;
    }

    RequestedScrollPosition = value;
    return true;
  }

  private bool SetRequestedView(float value)
  {
    if (!float.IsFinite(value) || value < 0f)
    {
      return false;
    }

    RequestedViewSize = value;
    return true;
  }

  private bool AttachScrollbar(UiElementId? scrollbarId)
  {
    if (scrollbarId.HasValue && !scrollbarId.Value.IsValid)
    {
      return false;
    }

    ScrollbarId = scrollbarId;
    return true;
  }

  private static float ValidateNonNegative(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}
