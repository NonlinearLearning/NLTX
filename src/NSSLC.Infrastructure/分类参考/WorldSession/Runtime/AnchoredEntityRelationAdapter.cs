namespace Terraria.WorldSession.Runtime;

public sealed class AnchoredEntityRelationAdapter
{
  private readonly HashSet<int> _sitting = new();
  private readonly HashSet<int> _sleeping = new();

  public IReadOnlyCollection<int> Sitting => _sitting;

  public IReadOnlyCollection<int> Sleeping => _sleeping;

  public void SetSitting(int entityId, bool value)
  {
    Set(_sitting, entityId, value);
  }

  public void SetSleeping(int entityId, bool value)
  {
    Set(_sleeping, entityId, value);
  }

  public void Clear()
  {
    _sitting.Clear();
    _sleeping.Clear();
  }

  private static void Set(HashSet<int> values, int entityId, bool value)
  {
    if (value)
    {
      values.Add(entityId);
    }
    else
    {
      values.Remove(entityId);
    }
  }
}
