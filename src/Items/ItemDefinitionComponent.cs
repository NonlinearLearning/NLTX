namespace Terraria.Items;

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
