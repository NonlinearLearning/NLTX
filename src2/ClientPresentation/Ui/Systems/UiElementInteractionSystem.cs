using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiElementInteractionSystem
{
  private readonly UiElementLifecycleSystem _lifecycle;

  public UiElementInteractionSystem(UiElementLifecycleSystem lifecycle)
  {
    _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
  }

  public bool SetHover(UiElementId id, bool isHovering)
  {
    if (!_lifecycle.TryGet(id, out UiElementInteractionComponent? component)
      || component is null
      || !component.IsInitialized
      || !component.IsActive)
    {
      return false;
    }

    component.SetMouseHovering(isHovering);
    return true;
  }

  public bool ClearHover(UiElementId id)
  {
    return SetHover(id, false);
  }
}
