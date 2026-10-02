namespace NLTX.PlayerInputGameplay.Creative;

public interface ICreativePowerNetworkAdapter
{
  void Publish(CreativePowerNetworkMessage message);
}

public readonly record struct CreativePowerNetworkMessage(
  ushort PowerId,
  int? PlayerSlot,
  float Value,
  bool Enabled,
  long Sequence);
