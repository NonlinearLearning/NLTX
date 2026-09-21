using Terraria.ClientPresentation.Ui.Commands;

namespace Terraria.ClientPresentation.Ui.Tree;

public sealed class UiElementTreeStore
{
  private readonly Dictionary<UiElementId, UiElementLayoutComponent> _nodes = [];
  private int _nextId = 1;

  public UiElementId CreateNode()
  {
    UiElementId id = new UiElementId(_nextId++);
    _nodes.Add(id, new UiElementLayoutComponent(id));
    return id;
  }

  public bool TryGet(UiElementId id, out UiElementLayoutComponent? component)
  {
    return _nodes.TryGetValue(id, out component);
  }

  public IReadOnlyList<UiElementId> RootNodes
  {
    get
    {
      return _nodes.Values
        .Where(node => !node.Parent.HasValue)
        .Select(node => node.Id)
        .OrderBy(id => id.Value)
        .ToArray();
    }
  }

  public bool Apply(UiElementTreeCommand command)
  {
    ArgumentNullException.ThrowIfNull(command);

    return command.Operation switch
    {
      UiElementTreeCommand.OperationKind.Attach => Attach(command.NodeId, command.ParentId),
      UiElementTreeCommand.OperationKind.Detach => Detach(command.NodeId),
      UiElementTreeCommand.OperationKind.Reparent => Reparent(command.NodeId, command.ParentId),
      UiElementTreeCommand.OperationKind.SetLayout => SetLayout(command.NodeId, command.Layout),
      _ => false
    };
  }

  internal IEnumerable<UiElementLayoutComponent> Nodes => _nodes.Values;

  private bool Attach(UiElementId nodeId, UiElementId? parentId)
  {
    if (!TryGetNode(nodeId, out UiElementLayoutComponent? node)
      || node is null
      || node.Parent.HasValue
      || !IsValidParent(nodeId, parentId))
    {
      return false;
    }

    if (parentId.HasValue)
    {
      UiElementLayoutComponent parent = _nodes[parentId.Value];
      parent.AddChild(nodeId);
      node.SetParent(parentId);
    }

    return true;
  }

  private bool Detach(UiElementId nodeId)
  {
    if (!TryGetNode(nodeId, out UiElementLayoutComponent? node)
      || node is null)
    {
      return false;
    }

    if (node.Parent.HasValue)
    {
      _nodes[node.Parent.Value].RemoveChild(nodeId);
    }

    foreach (UiElementId descendant in GetSubtree(nodeId).ToArray())
    {
      _nodes.Remove(descendant);
    }

    return true;
  }

  private bool Reparent(UiElementId nodeId, UiElementId? parentId)
  {
    if (!TryGetNode(nodeId, out UiElementLayoutComponent? node)
      || node is null
      || !IsValidParent(nodeId, parentId)
      || node.Parent == parentId)
    {
      return false;
    }

    if (node.Parent.HasValue)
    {
      _nodes[node.Parent.Value].RemoveChild(nodeId);
    }

    node.SetParent(parentId);
    if (parentId.HasValue)
    {
      _nodes[parentId.Value].AddChild(nodeId);
    }

    return true;
  }

  private bool SetLayout(
    UiElementId nodeId,
    UiElementTreeCommand.LayoutSettings? layout)
  {
    if (!TryGetNode(nodeId, out UiElementLayoutComponent? node)
      || node is null
      || layout is null)
    {
      return false;
    }

    node.SetConstraints(
      layout.Top,
      layout.Left,
      layout.Width,
      layout.Height,
      layout.MaxWidth,
      layout.MaxHeight,
      layout.MinWidth,
      layout.MinHeight,
      layout.PaddingTop,
      layout.PaddingLeft,
      layout.PaddingRight,
      layout.PaddingBottom,
      layout.MarginTop,
      layout.MarginLeft,
      layout.MarginRight,
      layout.MarginBottom,
      layout.HAlign,
      layout.VAlign);
    return true;
  }

  private bool IsValidParent(UiElementId nodeId, UiElementId? parentId)
  {
    return !parentId.HasValue
      || (parentId.Value != nodeId && _nodes.ContainsKey(parentId.Value)
        && !IsDescendant(nodeId, parentId.Value));
  }

  private bool IsDescendant(UiElementId candidate, UiElementId ancestor)
  {
    UiElementLayoutComponent node = _nodes[candidate];
    foreach (UiElementId child in node.Children)
    {
      if (child == ancestor || IsDescendant(child, ancestor))
      {
        return true;
      }
    }

    return false;
  }

  private IEnumerable<UiElementId> GetSubtree(UiElementId root)
  {
    yield return root;
    foreach (UiElementId child in _nodes[root].Children)
    {
      foreach (UiElementId descendant in GetSubtree(child))
      {
        yield return descendant;
      }
    }
  }

  private bool TryGetNode(UiElementId id, out UiElementLayoutComponent? component)
  {
    return _nodes.TryGetValue(id, out component);
  }
}
