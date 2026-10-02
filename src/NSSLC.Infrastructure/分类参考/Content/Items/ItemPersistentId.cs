namespace Terraria.Content.Items;

public sealed record ItemPersistentId
{
  public ItemPersistentId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A persistent item ID is required.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }
}
