namespace Terraria.Items;

public readonly record struct ExternalContentId(string Domain, int TypeId)
{
  public bool IsDefined => !string.IsNullOrWhiteSpace(Domain);
}
