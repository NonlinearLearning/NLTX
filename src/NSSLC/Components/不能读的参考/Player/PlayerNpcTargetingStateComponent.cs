namespace Terraria.Player;

/// <summary>
/// 保存玩家仇恨修正和不主动仇恨该玩家的 NPC 类型集合。
/// </summary>
/// <remarks>
/// <para>职责：保存玩家仇恨值和 NPC 类型免仇恨集合。</para>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：aggro（第 993 行）； npcTypeNoAggro（第 2374 行）。</para>
/// <para>
/// 用途：NPC 选敌流程读取 Aggro 和类型例外，决定是否以及多大程度优先选择此玩家。
/// </para>
/// <para>
/// 当前实现：Commit 先复制输入、检查 NPC 类型编号非负，再替换去重集合并提交 Aggro。HasNoAggroFor 按类型编号查询；目标选择由外部 AI 流程
/// 完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C137，L2（通用可选玩法）。分级理由：可选玩家仇恨加成和免仇恨表，普通敌人可按固定距离选择目标。
/// </para>
/// </remarks>
public sealed class PlayerNpcTargetingStateComponent
{
  private readonly HashSet<int> _noAggroNpcTypes = new();

  /// <summary>
  /// 玩家对 NPC 选敌的仇恨修正输入。
  /// </summary>
  public int Aggro { get; private set; }

  /// <summary>
  /// 查询指定 NPC 类型是否位于不仇恨例外集合。
  /// </summary>
  public bool HasNoAggroFor(int npcType)
  {
    return _noAggroNpcTypes.Contains(npcType);
  }

  /// <summary>
  /// 校验并整体替换 NPC 类型例外集合，同时提交仇恨修正。
  /// </summary>
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
