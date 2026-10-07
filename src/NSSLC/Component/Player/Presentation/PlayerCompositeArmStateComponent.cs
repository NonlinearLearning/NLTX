namespace Terraria.Player.Presentation;

/// <summary>
/// 保存玩家前后手臂的组合动画姿态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：compositeFrontArm（第 1340 行）； compositeBackArm（第 1342 行）。</para>
/// </remarks>
public sealed class PlayerCompositeArmStateComponent
{
  public PlayerCompositeArmSnapshot FrontArm { get; private set; }

  public PlayerCompositeArmSnapshot BackArm { get; private set; }

  internal void SetFrontArm(PlayerCompositeArmSnapshot arm)
  {
    FrontArm = arm;
  }

  internal void SetBackArm(PlayerCompositeArmSnapshot arm)
  {
    BackArm = arm;
  }

  internal void Reset()
  {
    FrontArm = default;
    BackArm = default;
  }

  public PlayerCompositeArmsSnapshot ToSnapshot()
  {
    return new PlayerCompositeArmsSnapshot(FrontArm, BackArm);
  }
}
