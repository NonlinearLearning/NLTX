namespace Terraria.Items;

/// <summary>
/// 保存物品堆叠数量、上限及合并键。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>主要源成员：stack（第 138 行）； maxStack（第 140 行）。</para>
/// <para>重组说明：StackKey 和无限堆叠标记属于 NLTX 堆叠规则表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 3023 行。</para>
/// </remarks>
public struct ItemStackComponent
{
  public ItemStackComponent(
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

  public bool IsEmpty => Quantity <= 0;

  public bool IsFull =>
    !IsUnlimited &&
    MaximumQuantity > 0 &&
    Quantity >= MaximumQuantity;

  public bool IsValid =>
    Quantity >= 0 &&
    (IsUnlimited || (MaximumQuantity >= 0 && Quantity <= MaximumQuantity));
}
