using System.Collections.Generic;

namespace Terraria.Items.Commerce;

/// <summary>
/// 保存商店商品报价和补货状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Chest.SetupShop 的商店商品生成流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>重组说明：商品报价集合、商店版本和补货时刻是独立商店模型新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-item-container-and-economy-component-code-draft.md。
/// </para>
/// <para>依据位置：第 876 行。</para>
/// </remarks>
public sealed class ShopInventoryComponent
{
  private readonly List<CommerceOffer> _offers;

  public ShopInventoryComponent(
    IReadOnlyList<CommerceOffer>? offers = null,
    long shopRevision = 0,
    long? restockAtTick = null,
    long? lastRestockTick = null)
  {
    _offers = offers is null
      ? []
      : new List<CommerceOffer>(offers);
    ShopRevision = shopRevision;
    RestockAtTick = restockAtTick;
    LastRestockTick = lastRestockTick;
  }

  public long ShopRevision;
  public long? RestockAtTick;
  public long? LastRestockTick;

  public IReadOnlyList<CommerceOffer> Offers => _offers;

  public bool IsRestockDueAt(long currentTick) =>
    RestockAtTick.HasValue &&
    currentTick >= RestockAtTick.Value;
}
