namespace Terraria.Player;

// Records the last persistence timestamp after an external save commit succeeds.
// It does not read clocks or perform file I/O itself.
public sealed class PlayerSaveCheckpointComponent
{
  public long LastSavedBinaryTimestamp { get; set; }
}
