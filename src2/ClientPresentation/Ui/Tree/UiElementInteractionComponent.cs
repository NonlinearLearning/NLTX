using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Tree;

public sealed class UiElementInteractionComponent
{
  public UiElementInteractionComponent(UiElementId id)
  {
    if (!id.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    Id = id;
  }

  public UiElementId Id { get; }

  public bool IsInitialized { get; private set; }

  public bool IsActive { get; private set; }

  public bool IgnoresMouseInteraction { get; private set; }

  public bool PassThroughMouseInteraction { get; private set; }

  public bool OverflowHidden { get; private set; }

  public bool UseImmediateMode { get; private set; }

  public UiSnapPoint? SnapPoint { get; private set; }

  public bool NoGamepadSupport { get; private set; }

  public bool IsMouseHovering { get; private set; }

  public bool IsDisposed { get; private set; }

  internal void Configure(
    bool ignoresMouseInteraction,
    bool passThroughMouseInteraction,
    bool overflowHidden,
    bool useImmediateMode,
    bool noGamepadSupport)
  {
    if (IsDisposed)
    {
      throw new InvalidOperationException("The UI element has been disposed.");
    }

    IgnoresMouseInteraction = ignoresMouseInteraction;
    PassThroughMouseInteraction = passThroughMouseInteraction;
    OverflowHidden = overflowHidden;
    UseImmediateMode = useImmediateMode;
    NoGamepadSupport = noGamepadSupport;
  }

  internal void MarkInitialized()
  {
    if (IsDisposed)
    {
      throw new InvalidOperationException("The UI element has been disposed.");
    }

    IsInitialized = true;
  }

  internal void SetActive(bool isActive)
  {
    if (IsDisposed)
    {
      throw new InvalidOperationException("The UI element has been disposed.");
    }

    IsActive = isActive;
    if (!isActive)
    {
      ClearTransientInteractionState();
    }
  }

  internal void SetSnapPoint(UiSnapPoint? snapPoint)
  {
    if (IsDisposed)
    {
      throw new InvalidOperationException("The UI element has been disposed.");
    }

    SnapPoint = snapPoint;
  }

  internal void SetMouseHovering(bool isMouseHovering)
  {
    if (IsDisposed || !IsInitialized || !IsActive || IgnoresMouseInteraction)
    {
      IsMouseHovering = false;
      return;
    }

    IsMouseHovering = isMouseHovering;
  }

  internal void ClearTransientInteractionState()
  {
    IsMouseHovering = false;
    SnapPoint = null;
  }

  internal void Dispose()
  {
    if (IsDisposed)
    {
      return;
    }

    IsActive = false;
    IsInitialized = false;
    ClearTransientInteractionState();
    IsDisposed = true;
  }
}
