namespace Terraria.Content.Items;

public sealed record ItemPresentationDefinition
{
  public ItemPresentationDefinition(int rarityTier = 0)
  {
    RarityTier = rarityTier;
  }

  public int RarityTier { get; }
}
