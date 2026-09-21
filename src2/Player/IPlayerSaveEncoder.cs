namespace Terraria.NonAuthoritative.Player;

public interface IPlayerSaveEncoder
{
  PlayerSaveEncodeResult Encode(PlayerSaveSessionSnapshot session, PlayerSaveCommand command);
}
