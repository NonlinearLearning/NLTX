namespace Terraria.Player;

/// <summary>
/// Player-owned aggro inputs consumed by NPC target selection.
/// </summary>
/// <remarks>
/// <para>职责：保存玩家仇恨值和 NPC 类型免仇恨集合。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：aggro（第 993 行）； npcTypeNoAggro（第 2374 行）。</para>
/// </remarks>
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
