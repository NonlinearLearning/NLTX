using System;

namespace Terraria.Town.Housing;

/// <summary>
/// 保存城镇 NPC 住房位置、无家状态及搜索计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：homeless（第 6403 行）； homelessDespawn（第 6405 行）； lookForHomeTimeout（第 6407 行）； homeTileX（第
/// 6411 行）； homeTileY（第 6413 行）； housingCategory（第 6415 行）； oldHomeless（第 6417 行）； oldHomeTileX（第
/// 6419 行）； oldHomeTileY（第 6421 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 140 行。</para>
/// </remarks>
public sealed class TownHousingRelationStateComponent
{
  public bool IsHomeless { get; private set; }

  public bool HomelessDespawn { get; private set; }

  public int LookForHomeTimeout { get; private set; }

  public TownRoomTilePoint? HomeTile { get; private set; }

  public int HousingCategory { get; private set; }

  public bool OldHomeless { get; private set; }

  public TownRoomTilePoint? OldHomeTile { get; private set; }

  public bool HasHome => HomeTile.HasValue && !IsHomeless;

  public void CommitRelation(
    bool isHomeless,
    bool homelessDespawn,
    int lookForHomeTimeout,
    TownRoomTilePoint? homeTile,
    int housingCategory)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(lookForHomeTimeout);
    if (isHomeless && homeTile.HasValue)
    {
      throw new ArgumentException(
        "A homeless resident cannot have an assigned home tile.",
        nameof(homeTile));
    }

    CaptureCompatibilitySnapshot();
    IsHomeless = isHomeless;
    HomelessDespawn = homelessDespawn;
    LookForHomeTimeout = lookForHomeTimeout;
    HomeTile = homeTile;
    HousingCategory = housingCategory;
  }

  public void Reset()
  {
    IsHomeless = false;
    HomelessDespawn = false;
    LookForHomeTimeout = 0;
    HomeTile = null;
    HousingCategory = 0;
    OldHomeless = false;
    OldHomeTile = null;
  }

  private void CaptureCompatibilitySnapshot()
  {
    OldHomeless = IsHomeless;
    OldHomeTile = HomeTile;
  }
}
