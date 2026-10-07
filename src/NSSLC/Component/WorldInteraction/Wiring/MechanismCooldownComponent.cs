using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.Wiring;

/// <summary>
/// 保存机关触发位置、计时和发射器冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Wiring。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Wiring.cs。</para>
/// <para>
/// 主要源成员：_mechX（第 57 行）； _mechY（第 59 行）； _numMechs（第 61 行）； _mechTime（第 63 行）； cannonCoolDown（第
/// 69 行）； bunnyCannonCoolDown（第 71 行）； snowballCannonCoolDown（第 73 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 626 行。</para>
/// </remarks>
public sealed class MechanismCooldownComponent
{
  private readonly List<MechanismCooldownEntry> _entries = new();
  private readonly ReadOnlyCollection<MechanismCooldownEntry> _entriesView;

  public MechanismCooldownComponent()
  {
    _entriesView = _entries.AsReadOnly();
  }

  public IReadOnlyList<MechanismCooldownEntry> Entries => _entriesView;
  public int CannonCooldownTicks { get; internal set; }
  public int BunnyCannonCooldownTicks { get; internal set; }
  public int SnowballCannonCooldownTicks { get; internal set; }
}
