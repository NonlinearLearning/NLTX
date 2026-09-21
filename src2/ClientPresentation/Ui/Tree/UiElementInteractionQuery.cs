using Terraria.ClientPresentation.Ui.Systems;

namespace Terraria.ClientPresentation.Ui.Tree;

public static class UiElementInteractionQuery
{
  public static bool TryGet(
    UiElementLifecycleSystem lifecycle,
    UiElementId id,
    out Snapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    if (!lifecycle.TryGet(id, out UiElementInteractionComponent? component)
      || component is null)
    {
      snapshot = default;
      return false;
    }

    snapshot = new Snapshot(
      component.Id,
      component.IsInitialized,
      component.IsActive,
      component.IgnoresMouseInteraction,
      component.PassThroughMouseInteraction,
      component.OverflowHidden,
      component.UseImmediateMode,
      component.SnapPoint,
      component.NoGamepadSupport,
      component.IsMouseHovering);
    return true;
  }

  public static bool CanReceiveMouseInteraction(
    UiElementLifecycleSystem lifecycle,
    UiElementId id)
  {
    return TryGet(lifecycle, id, out Snapshot snapshot)
      && snapshot.IsInitialized
      && snapshot.IsActive
      && !snapshot.IgnoresMouseInteraction;
  }

  public static bool BlocksMouseInteraction(
    UiElementLifecycleSystem lifecycle,
    UiElementId id)
  {
    return TryGet(lifecycle, id, out Snapshot snapshot)
      && snapshot.IsInitialized
      && snapshot.IsActive
      && !snapshot.IgnoresMouseInteraction
      && !snapshot.PassThroughMouseInteraction;
  }

  public readonly record struct Snapshot(
    UiElementId Id,
    bool IsInitialized,
    bool IsActive,
    bool IgnoresMouseInteraction,
    bool PassThroughMouseInteraction,
    bool OverflowHidden,
    bool UseImmediateMode,
    Terraria.ClientPresentation.Ui.Layout.UiSnapPoint? SnapPoint,
    bool NoGamepadSupport,
    bool IsMouseHovering);
}
