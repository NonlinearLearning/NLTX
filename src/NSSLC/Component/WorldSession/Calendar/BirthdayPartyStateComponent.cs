using System;
using System.Collections.Generic;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界生日派对的触发、冷却和参与 NPC 状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.BirthdayParty。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/BirthdayParty.cs。</para>
/// <para>
/// 主要源成员：ManualParty（第 13 行）； GenuineParty（第 15 行）； PartyDaysOnCooldown（第 17 行）；
/// _wasCelebrating（第 21 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 1109 行。</para>
/// </remarks>
public sealed class BirthdayPartyStateComponent
{
  private readonly IReadOnlyList<NpcEntityId> _celebratingNpcIds;

  public BirthdayPartyStateComponent(
    bool manualParty = false,
    bool genuineParty = false,
    int partyDaysOnCooldown = 0,
    IReadOnlyList<NpcEntityId>? celebratingNpcIds = null,
    bool wasCelebrating = false)
  {
    ManualParty = manualParty;
    GenuineParty = genuineParty;
    PartyDaysOnCooldown = partyDaysOnCooldown;
    _celebratingNpcIds = new List<NpcEntityId>(
      celebratingNpcIds ?? Array.Empty<NpcEntityId>()).AsReadOnly();
    WasCelebrating = wasCelebrating;
    Validate();
  }

  public bool ManualParty;
  public bool GenuineParty;
  public int PartyDaysOnCooldown;
  public IReadOnlyList<NpcEntityId> CelebratingNpcIds => _celebratingNpcIds;
  public bool WasCelebrating;

  public bool IsUp => ManualParty || GenuineParty;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(PartyDaysOnCooldown);

    HashSet<NpcEntityId> ids = new();
    for (int index = 0; index < CelebratingNpcIds.Count; index++)
    {
      if (!ids.Add(CelebratingNpcIds[index]))
      {
        throw new ArgumentException(
          "A birthday party cannot contain a duplicate NPC reference.",
          nameof(CelebratingNpcIds));
      }
    }
  }
}
