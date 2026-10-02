namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueConsumeSystem
{
  private readonly ConditionalDialogueCatalog _catalog;
  private readonly ConditionalDialogueConsumptionStateComponent _state;

  public ConditionalDialogueConsumeSystem(ConditionalDialogueCatalog catalog)
    : this(catalog, new ConditionalDialogueConsumptionStateComponent())
  {
  }

  public ConditionalDialogueConsumeSystem(
    ConditionalDialogueCatalog catalog,
    ConditionalDialogueConsumptionStateComponent state)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    _state = state ?? throw new ArgumentNullException(nameof(state));
  }

  public ConditionalDialogueConsumeResult Consume(ConsumeConditionalDialogueCommand command)
  {
    if (!_catalog.TryGet(command.NpcType, command.Key, out ConditionalDialogueDefinition definition) ||
      !definition.Condition.Invoke(new NpcDialogueReadView(command.NpcType, command.ExpectedRevision)))
    {
      return new ConditionalDialogueConsumeResult(false, false, _state.Revision);
    }

    bool applied = _state.TryConsume(command);
    return new ConditionalDialogueConsumeResult(applied, !applied, _state.Revision);
  }
}
