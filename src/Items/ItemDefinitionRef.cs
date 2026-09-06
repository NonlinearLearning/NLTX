namespace Terraria.Items;

public readonly record struct ItemDefinitionRef(
  ExternalContentId ContentId,
  int DefinitionRevision)
{
  public bool IsKnown => ContentId.IsDefined;
}
