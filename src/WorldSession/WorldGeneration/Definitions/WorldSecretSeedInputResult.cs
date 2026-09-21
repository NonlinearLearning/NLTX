namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedInputResult
{
  private WorldSecretSeedInputResult(
    WorldSecretSeedInputMatch match,
    WorldSecretSeedInputRejectionReason? rejectionReason)
  {
    Match = match;
    RejectionReason = rejectionReason;
  }

  public bool IsMatch => RejectionReason is null;

  public WorldSecretSeedInputMatch Match { get; }

  public WorldSecretSeedInputRejectionReason? RejectionReason { get; }

  public static WorldSecretSeedInputResult Matched(WorldSecretSeedInputMatch match)
  {
    return new WorldSecretSeedInputResult(match, null);
  }

  public static WorldSecretSeedInputResult Rejected(
    WorldSecretSeedInputRejectionReason reason)
  {
    return new WorldSecretSeedInputResult(default, reason);
  }
}
