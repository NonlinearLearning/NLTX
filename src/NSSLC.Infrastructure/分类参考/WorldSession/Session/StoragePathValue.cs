namespace Terraria.WorldSession.Session;

public readonly record struct StoragePathValue
{
  public string Value { get; }

  public StoragePathValue(string value)
  {
    Value = string.IsNullOrWhiteSpace(value)
      ? throw new ArgumentException("A storage path is required.", nameof(value))
      : value;
  }
}
