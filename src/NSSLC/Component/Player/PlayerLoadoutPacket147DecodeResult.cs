namespace Terraria.Player;

public readonly record struct PlayerLoadoutPacket147DecodeResult(
  PlayerLoadoutPacket147DecodeStatus Status,
  PlayerLoadoutNetworkRequest? Request)
{
  public bool Decoded => Status == PlayerLoadoutPacket147DecodeStatus.Decoded;

  public static PlayerLoadoutPacket147DecodeResult Truncated()
  {
    return new PlayerLoadoutPacket147DecodeResult(
      PlayerLoadoutPacket147DecodeStatus.Truncated,
      null);
  }
}
