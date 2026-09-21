namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldSaveEncoder
{
  WorldSaveEncodeResult Encode(WorldSaveCommand command);
}
