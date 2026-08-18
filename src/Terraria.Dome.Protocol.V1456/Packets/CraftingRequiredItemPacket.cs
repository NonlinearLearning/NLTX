namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct CraftingRequiredItemPacket(
  int ItemIdOrRecipeGroup,
  int Stack);
