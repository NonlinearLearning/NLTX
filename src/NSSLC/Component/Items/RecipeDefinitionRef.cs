namespace Terraria.Items;

public readonly record struct RecipeDefinitionRef(
  ExternalContentId ContentId,
  int DefinitionRevision)
{
  public bool IsKnown => ContentId.IsDefined;
}
