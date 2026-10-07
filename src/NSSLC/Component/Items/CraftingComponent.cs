using System.Collections.Generic;

namespace Terraria.Items;

/// <summary>
/// 保存当前合成配方、请求数量和已预留材料。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Recipe 的配方判定、材料消费与产物创建流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Recipe.cs。</para>
/// <para>重组说明：材料预留、事务阶段、来源版本和事务序号是合成流程拆分时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-item-container-and-economy-component-design.md。</para>
/// <para>依据位置：第 703 行。</para>
/// </remarks>
public sealed class CraftingComponent
{
  private readonly List<CraftingMaterialReservation> _reservedMaterials;

  public CraftingComponent(
    int activeRecipeId = 0,
    int requestedQuantity = 0,
    long acceptedAtTick = 0,
    long materialInventoryRevision = 0,
    IReadOnlyList<CraftingMaterialReservation>? reservedMaterials = null,
    long craftSequence = 0)
  {
    ActiveRecipeId = activeRecipeId;
    RequestedQuantity = requestedQuantity;
    AcceptedAtTick = acceptedAtTick;
    MaterialInventoryRevision = materialInventoryRevision;
    _reservedMaterials = reservedMaterials is null
      ? []
      : new List<CraftingMaterialReservation>(reservedMaterials);
    CraftSequence = craftSequence;
  }

  public int ActiveRecipeId;
  public int RequestedQuantity;
  public long AcceptedAtTick;
  public long MaterialInventoryRevision;
  public long CraftSequence;

  public IReadOnlyList<CraftingMaterialReservation> ReservedMaterials => _reservedMaterials;
  public bool HasActiveCraft => ActiveRecipeId > 0 && RequestedQuantity > 0;
}
