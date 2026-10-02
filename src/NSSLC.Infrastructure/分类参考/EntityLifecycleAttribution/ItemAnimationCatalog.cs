using System.Collections.ObjectModel;

namespace Terraria.EntityLifecycleAttribution;

public sealed class ItemAnimationCatalog
{
  private readonly Dictionary<int, ItemAnimationDefinition> _definitions = new();

  public IReadOnlyCollection<int> RegisteredContentTypes =>
    new ReadOnlyCollection<int>(_definitions.Keys.OrderBy(key => key).ToArray());

  public bool Register(ItemAnimationDefinition definition)
  {
    return _definitions.TryAdd(definition.ContentType, definition);
  }

  public bool TryGet(int contentType, out ItemAnimationDefinition definition)
  {
    return _definitions.TryGetValue(contentType, out definition);
  }

  public void Clear()
  {
    _definitions.Clear();
  }
}
