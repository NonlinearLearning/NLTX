namespace Terraria.Dome.Protocol.V1456.Packets;

public enum CreativePowerPayloadKind : byte
{
  SharedButton,
  SharedToggle,
  SharedSlider,
  PerPlayerToggle,
  PerPlayerToggleSyncEveryone,
  PerPlayerToggleSyncOnePlayer,
  PerPlayerSlider
}
