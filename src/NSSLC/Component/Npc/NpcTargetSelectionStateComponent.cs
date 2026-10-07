namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 选目标后的几何、评分、朝向和同步结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：netUpdate（第 6015 行）； directionY（第 6305 行）； target（第 6321 行）； targetRect（第 6345 行）；
/// oldTarget（第 6363 行）。
/// </para>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：direction（第 20 行）。</para>
/// <para>重组说明：Score 和目标种类用于显式选目标结果，来自选目标流程的重组。</para>
/// </remarks>
public sealed class NpcTargetSelectionStateComponent
{
  public NpcTargetKind TargetKind { get; private set; }

  public int LegacyTargetIndex { get; private set; } = -1;

  public int SecondaryLegacySlot { get; private set; } = -1;

  public NpcTargetGeometrySnapshot TargetGeometry { get; private set; }

  public float Score { get; private set; } = float.PositiveInfinity;

  public int Direction { get; private set; }

  public int DirectionY { get; private set; }

  public bool NetUpdateRequested { get; private set; }

  public void Commit(in NpcTargetSelectionResult result)
  {
    TargetKind = result.TargetKind;
    LegacyTargetIndex = result.LegacyTargetIndex;
    SecondaryLegacySlot = result.SecondaryLegacySlot;
    TargetGeometry = result.TargetGeometry;
    Score = result.Score;
    Direction = result.Direction;
    DirectionY = result.DirectionY;
    NetUpdateRequested = result.NetUpdateRequested;
  }

  internal bool ResetForTermination()
  {
    bool changed = TargetKind != NpcTargetKind.None ||
      LegacyTargetIndex != -1 ||
      SecondaryLegacySlot != -1 ||
      TargetGeometry != default ||
      !float.IsPositiveInfinity(Score) ||
      Direction != 0 ||
      DirectionY != 0 ||
      NetUpdateRequested;

    TargetKind = NpcTargetKind.None;
    LegacyTargetIndex = -1;
    SecondaryLegacySlot = -1;
    TargetGeometry = default;
    Score = float.PositiveInfinity;
    Direction = 0;
    DirectionY = 0;
    NetUpdateRequested = false;
    return changed;
  }
}
