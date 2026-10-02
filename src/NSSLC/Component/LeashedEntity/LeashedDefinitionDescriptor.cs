namespace Terraria.LeashedEntity;

public enum LeashedDefinitionKind : byte
{
  Critter,
  Kite
}

public sealed record LeashedDefinitionDescriptor
{
  public LeashedDefinitionDescriptor(
    int definitionId,
    string key,
    LeashedDefinitionKind kind,
    int? contentId)
  {
    if (definitionId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definitionId));
    }

    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("A definition key is required.", nameof(key));
    }

    if (contentId is <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contentId));
    }

    DefinitionId = definitionId;
    Key = key;
    Kind = kind;
    ContentId = contentId;
  }

  public int DefinitionId { get; }

  public string Key { get; }

  public LeashedDefinitionKind Kind { get; }

  public int? ContentId { get; }
}

