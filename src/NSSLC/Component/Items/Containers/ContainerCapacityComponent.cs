namespace Terraria.Items;

/// <summary>
/// 保存容器种类、槽位数量及容量限制。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Chest 的槽位容量与容器内容模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Chest.cs。</para>
/// <para>重组说明：最大重量和嵌套容器策略属于 NLTX 容器模型扩展，不对应原版独立字段。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-item-container-and-economy-component-code-draft.md。
/// </para>
/// <para>依据位置：第 386 行。</para>
/// </remarks>
public sealed class ContainerCapacityComponent
{
  public ContainerCapacityComponent(
    ContainerKind kind,
    int slotCount,
    long? maximumWeight = null,
    bool allowsNestedContainers = false)
  {
    Kind = kind;
    SlotCount = slotCount;
    MaximumWeight = maximumWeight;
    AllowsNestedContainers = allowsNestedContainers;
  }

  public ContainerKind Kind;
  public int SlotCount;
  public long? MaximumWeight;
  public bool AllowsNestedContainers;

  public bool IsValid =>
    SlotCount >= 0 &&
    (!MaximumWeight.HasValue || MaximumWeight.Value >= 0);
}
