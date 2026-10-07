using System.Collections.Frozen;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.PressurePlates;

/// <summary>
/// 保存压力板上的玩家占用集合及首次更新状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.PressurePlateHelper。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/PressurePlateHelper.cs。</para>
/// <para>主要源成员：PressurePlatesPressed（第 11 行）； NeedsFirstUpdate（第 13 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 672 行。</para>
/// </remarks>
public sealed class PressurePlateOccupancyComponent
{
  private readonly Dictionary<TileCoordinate, HashSet<EntityReference>> _occupantsByPlate = new();

  public IReadOnlyDictionary<TileCoordinate, IReadOnlySet<EntityReference>> OccupantsByPlate
  {
    get
    {
      var occupants = new Dictionary<TileCoordinate, IReadOnlySet<EntityReference>>();
      foreach ((TileCoordinate coordinate, HashSet<EntityReference> occupantsAtPlate) in _occupantsByPlate)
      {
        occupants.Add(coordinate, occupantsAtPlate.ToFrozenSet());
      }

      return occupants.ToFrozenDictionary();
    }
  }

  public bool NeedsFirstUpdate { get; internal set; }
}
