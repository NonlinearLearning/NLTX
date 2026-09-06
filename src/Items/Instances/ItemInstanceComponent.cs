namespace Terraria.Items;

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
