namespace Terraria.NpcTownBestiary;

public sealed class ConditionalDialogueRegistrationSystem
{
  private readonly ConditionalDialogueCatalog _catalog;

  public ConditionalDialogueRegistrationSystem(ConditionalDialogueCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public void Register(ConditionalDialogueDefinition definition)
  {
    _catalog.Register(definition);
  }
}
