namespace Terraria.Items;

public readonly record struct LootSourceRef(
  ExternalContentId SourceId,
  long SourceRevision)
{
  public bool IsKnown => SourceId.IsDefined;
}
