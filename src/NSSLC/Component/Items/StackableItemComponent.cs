namespace Terraria.Items;

/// <summary>
/// 保存可堆叠物品的数量、上限和合并条件。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>主要源成员：stack（第 138 行）； maxStack（第 140 行）。</para>
/// <para>重组说明：StackKey 和无限堆叠标记属于 NLTX 堆叠规则表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-item-container-and-economy-component-design.md。</para>
/// <para>依据位置：第 227 行。</para>
/// </remarks>
public struct StackableItemComponent
{
  public StackableItemComponent(
    int quantity,
    int maximumQuantity,
    ulong stackKey = 0,
    bool isUnlimited = false)
  {
    Quantity = quantity;
    MaximumQuantity = maximumQuantity;
    StackKey = stackKey;
    IsUnlimited = isUnlimited;
  }

  public int Quantity;
  public int MaximumQuantity;
  public ulong StackKey;
  public bool IsUnlimited;

  public int RemainingCapacity => Math.Max(0, MaximumQuantity - Quantity);
  public bool IsEmpty => Quantity <= 0;
  public bool IsFull => Quantity >= MaximumQuantity;
}
