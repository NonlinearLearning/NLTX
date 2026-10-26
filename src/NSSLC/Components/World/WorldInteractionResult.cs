namespace Terraria.World;

public readonly record struct WorldInteractionResult(
    bool Succeeded,
    string? Failure = null) {
  public static WorldInteractionResult Success() => new(true);

  public static WorldInteractionResult Rejected(string failure) {
    ArgumentException.ThrowIfNullOrWhiteSpace(failure);
    return new(false, failure);
  }
}
