namespace Terraria.NpcTownBestiary;

public readonly record struct ConsumeConditionalDialogueCommand
{
  public ConsumeConditionalDialogueCommand(
    NpcTypeId npcType,
    ConditionalDialogueKey key,
    ulong expectedRevision)
  {
    NpcType = npcType;
    Key = key;
    ExpectedRevision = expectedRevision;
  }

  public NpcTypeId NpcType { get; }

  public ConditionalDialogueKey Key { get; }

  public ulong ExpectedRevision { get; }
}
