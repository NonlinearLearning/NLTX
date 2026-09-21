namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldSaveValidationQuery
{
  private readonly Func<ReadOnlyMemory<byte>, bool> _validator;

  public WorldSaveValidationQuery(Func<ReadOnlyMemory<byte>, bool> validator)
  {
    _validator = validator ?? throw new ArgumentNullException(nameof(validator));
  }

  public bool IsValid(ReadOnlyMemory<byte> bytes)
  {
    return _validator(bytes);
  }
}
