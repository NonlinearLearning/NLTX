namespace Terraria.Player.Progression;

// status: local-rebuild-verified; production-integration: unknown
/// <summary>
/// 保存玩家常规召唤物的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：pygmy（第 838 行）； raven（第 840 行）； slime（第 842 行）； hornetMinion（第 844 行）； impMinion（第 846
/// 行）； twinsMinion（第 848 行）； spiderMinion（第 850 行）； pirateMinion（第 852 行）； sharknadoMinion（第 854
/// 行）； UFOMinion（第 856 行）； DeadlySphereMinion（第 858 行）； stardustMinion（第 860 行）；
/// stardustGuardian（第 862 行）； stardustDragon（第 864 行）； batsOfLight（第 866 行）； babyBird（第 868 行）；
/// vampireFrog（第 870 行）； stormTiger（第 872 行）； smolstar（第 876 行）； empressBlade（第 878 行）；
/// flinxMinion（第 880 行）； abigailMinion（第 882 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 191 行。</para>
/// </remarks>
public sealed class PlayerCoreMinionCapabilityComponent
{
  public bool Pygmy { get; internal set; }

  public bool Raven { get; internal set; }

  public bool Slime { get; internal set; }

  public bool HornetMinion { get; internal set; }

  public bool ImpMinion { get; internal set; }

  public bool TwinsMinion { get; internal set; }

  public bool SpiderMinion { get; internal set; }

  public bool PirateMinion { get; internal set; }

  public bool SharknadoMinion { get; internal set; }

  public bool UfoMinion { get; internal set; }

  public bool DeadlySphereMinion { get; internal set; }

  public bool StardustMinion { get; internal set; }

  public bool StardustGuardian { get; internal set; }

  public bool StardustDragon { get; internal set; }

  public bool BatsOfLight { get; internal set; }

  public bool BabyBird { get; internal set; }

  public bool VampireFrog { get; internal set; }

  public bool StormTiger { get; internal set; }

  public bool Smolstar { get; internal set; }

  public bool EmpressBlade { get; internal set; }

  public bool FlinxMinion { get; internal set; }

  public bool AbigailMinion { get; internal set; }
}
