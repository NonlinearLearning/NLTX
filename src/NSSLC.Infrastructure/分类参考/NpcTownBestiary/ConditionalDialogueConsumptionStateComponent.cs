namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueConsumptionStateComponent
{
  private readonly HashSet<(NpcTypeId NpcType, ConditionalDialogueKey Key)> _consumed = new();

  public ulong Revision { get; private set; }

  public bool IsConsumed(NpcTypeId npcType, ConditionalDialogueKey key)
  {
    return _consumed.Contains((npcType, key));
  }

  internal bool TryConsume(ConsumeConditionalDialogueCommand command)
  {
    if (command.ExpectedRevision != Revision || !_consumed.Add((command.NpcType, command.Key)))
    {
      return false;
    }

    Revision++;
    return true;
  }
}
