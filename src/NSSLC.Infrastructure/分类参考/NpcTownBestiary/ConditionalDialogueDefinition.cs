namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueDefinition
{
  public ConditionalDialogueDefinition(
    ConditionalDialogueKey key,
    NpcTypeId npcType,
    Func<NpcDialogueReadView, bool> condition,
    bool showIndicator)
  {
    ArgumentNullException.ThrowIfNull(condition);
    Key = key;
    NpcType = npcType;
    Condition = condition;
    ShowIndicator = showIndicator;
  }

  public ConditionalDialogueKey Key { get; }

  public NpcTypeId NpcType { get; }

  public Func<NpcDialogueReadView, bool> Condition { get; }

  public bool ShowIndicator { get; }
}
