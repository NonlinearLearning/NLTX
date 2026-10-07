namespace Terraria.Player;

/// <summary>
/// Player-owned aggro inputs consumed by NPC target selection.
/// </summary>
public sealed class PlayerNpcTargetingStateComponent
{
  private readonly HashSet<int> _noAggroNpcTypes = new();

  public int Aggro { get; private set; }

  public bool HasNoAggroFor(int npcType)
  {
    return _noAggroNpcTypes.Contains(npcType);
  }

  public void Commit(int aggro, IEnumerable<int> noAggroNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(noAggroNpcTypes);
    HashSet<int> committedNoAggroTypes = new(noAggroNpcTypes);
    foreach (int npcType in committedNoAggroTypes)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(npcType);
    }

    _noAggroNpcTypes.Clear();
    foreach (int npcType in committedNoAggroTypes)
    {
      _noAggroNpcTypes.Add(npcType);
    }
    Aggro = aggro;
  }
}
