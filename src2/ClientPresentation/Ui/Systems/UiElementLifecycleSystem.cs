using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiElementLifecycleSystem
{
  private readonly UiElementTreeStore _store;
  private readonly Dictionary<UiElementId, UiElementInteractionComponent> _components = [];

  public UiElementLifecycleSystem(UiElementTreeStore store)
  {
    _store = store ?? throw new ArgumentNullException(nameof(store));
  }

  public bool Initialize(
    UiElementId id,
    bool ignoresMouseInteraction = false,
    bool passThroughMouseInteraction = false,
    bool overflowHidden = false,
    bool useImmediateMode = false,
    bool noGamepadSupport = false)
  {
    if (!TryGetLayoutNode(id, out _))
    {
      return false;
    }

    if (_components.TryGetValue(id, out UiElementInteractionComponent? existing))
    {
      return existing.IsInitialized;
    }

    InitializeSubtree(
      id,
      ignoresMouseInteraction,
      passThroughMouseInteraction,
      overflowHidden,
      useImmediateMode,
      noGamepadSupport);
    return true;
  }

  public bool Activate(UiElementId id)
  {
    if (!TryGetComponent(id, out UiElementInteractionComponent? component)
      || component is null
      || !component.IsInitialized)
    {
      return false;
    }

    SetActiveSubtree(id, true);
    return true;
  }

  public bool Deactivate(UiElementId id)
  {
    if (!TryGetComponent(id, out UiElementInteractionComponent? component)
      || component is null
      || !component.IsInitialized)
    {
      return false;
    }

    SetActiveSubtree(id, false);
    return true;
  }

  public bool SetSnapPoint(UiElementId id, UiSnapPoint? snapPoint)
  {
    if (!TryGetComponent(id, out UiElementInteractionComponent? component)
      || component is null
      || !component.IsInitialized
      || !component.IsActive)
    {
      return false;
    }

    component.SetSnapPoint(snapPoint);
    return true;
  }

  public bool Dispose(UiElementId id)
  {
    if (!TryGetComponent(id, out UiElementInteractionComponent? component)
      || component is null)
    {
      return false;
    }

    foreach (UiElementId descendant in GetSubtree(id).ToArray())
    {
      if (_components.Remove(descendant, out UiElementInteractionComponent? child))
      {
        child.Dispose();
      }
    }

    component.Dispose();
    _components.Remove(id);
    return true;
  }

  public bool TryGet(
    UiElementId id,
    out UiElementInteractionComponent? component)
  {
    return _components.TryGetValue(id, out component);
  }

  internal IEnumerable<UiElementInteractionComponent> Components => _components.Values;

  private void InitializeSubtree(
    UiElementId id,
    bool ignoresMouseInteraction,
    bool passThroughMouseInteraction,
    bool overflowHidden,
    bool useImmediateMode,
    bool noGamepadSupport)
  {
    if (TryGetLayoutNode(id, out UiElementLayoutComponent? node)
      && node is not null)
    {
      UiElementInteractionComponent component = new(id);
      component.Configure(
        ignoresMouseInteraction,
        passThroughMouseInteraction,
        overflowHidden,
        useImmediateMode,
        noGamepadSupport);
      component.MarkInitialized();
      _components.Add(id, component);

      foreach (UiElementId childId in node.Children)
      {
        InitializeSubtree(
          childId,
          false,
          false,
          false,
          false,
          false);
      }
    }
  }

  private void SetActiveSubtree(UiElementId id, bool isActive)
  {
    if (TryGetComponent(id, out UiElementInteractionComponent? component)
      && component is not null)
    {
      component.SetActive(isActive);
    }

    if (TryGetLayoutNode(id, out UiElementLayoutComponent? node)
      && node is not null)
    {
      foreach (UiElementId childId in node.Children)
      {
        SetActiveSubtree(childId, isActive);
      }
    }
  }

  private IEnumerable<UiElementId> GetSubtree(UiElementId root)
  {
    yield return root;
    if (TryGetLayoutNode(root, out UiElementLayoutComponent? node)
      && node is not null)
    {
      foreach (UiElementId childId in node.Children)
      {
        foreach (UiElementId descendant in GetSubtree(childId))
        {
          yield return descendant;
        }
      }
    }
  }

  private bool TryGetLayoutNode(
    UiElementId id,
    out UiElementLayoutComponent? component)
  {
    return _store.TryGet(id, out component) && component is not null;
  }

  private bool TryGetComponent(
    UiElementId id,
    out UiElementInteractionComponent? component)
  {
    return _components.TryGetValue(id, out component) && component is not null;
  }
}
