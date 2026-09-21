namespace Terraria.Content.Items;

public sealed record ItemExternalId
{
  public ItemExternalId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("An external item ID is required.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }
}
