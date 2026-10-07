namespace Terraria.Items;

/// <summary>
/// 保存物品内容定义的编号和定义版本。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>主要源成员：type（第 122 行）。</para>
/// <para>重组说明：内容定义引用和 DefinitionRevision 是定义与实例分离后新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-item-container-and-economy-component-design.md。</para>
/// <para>依据位置：第 1045 行。</para>
/// </remarks>
public struct ItemDefinitionComponent
{
  public ItemDefinitionComponent(int contentId, int definitionRevision = 0)
  {
    ContentId = contentId;
    DefinitionRevision = definitionRevision;
  }

  public int ContentId;
  public int DefinitionRevision;

  public bool HasDefinition => ContentId > 0;

  // Retained for callers that still use the legacy Terraria item-type name.
  public int Type
  {
    get => ContentId;
    set => ContentId = value;
  }
}
