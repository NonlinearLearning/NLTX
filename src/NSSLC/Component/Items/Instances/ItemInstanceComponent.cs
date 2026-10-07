namespace Terraria.Items;

/// <summary>
/// 保存物品实例的定义引用、前缀、变体、染色和自定义名称。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Item。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Item.cs。</para>
/// <para>
/// 主要源成员：_nameOverride（第 32 行）； type（第 122 行）； favorited（第 124 行）； prefix（第 286 行）。
/// </para>
/// <para>重组说明：PersistentInstanceId、定义引用及变体关联是物品实例模型新增的表达。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-item-container-and-economy-component-code-draft.md。
/// </para>
/// <para>依据位置：第 234 行。</para>
/// </remarks>
public sealed class ItemInstanceComponent
{
  public ItemInstanceComponent(
    ItemDefinitionRef definitionRef,
    PersistentItemId persistentInstanceId,
    int prefixId = 0,
    ExternalContentId variantId = default,
    int dyeId = 0,
    bool isFavorited = false,
    string? nameOverride = null)
  {
    DefinitionRef = definitionRef;
    PersistentInstanceId = persistentInstanceId;
    PrefixId = prefixId;
    VariantId = variantId;
    DyeId = dyeId;
    IsFavorited = isFavorited;
    NameOverride = nameOverride;
  }

  public ItemDefinitionRef DefinitionRef;
  public PersistentItemId PersistentInstanceId;
  public int PrefixId;
  public ExternalContentId VariantId;
  public int DyeId;
  public bool IsFavorited;
  public string? NameOverride;

  public bool HasDefinition => DefinitionRef.IsKnown;
  public bool HasPersistentIdentity => PersistentInstanceId.IsAssigned;
  public bool HasVariant => VariantId.IsDefined;
  public bool HasNameOverride => !string.IsNullOrWhiteSpace(NameOverride);
}
