namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ItemPrefixChangedEvent(
  ushort ItemType,
  ushort PreviousPrefixId,
  ushort PrefixId);
