namespace Terraria.Content;

public sealed record TileInteractionDefinition(
  bool Container,
  bool Sign,
  bool Table = false,
  bool Rope = false,
  bool LavaDeath = false,
  bool WaterDeath = false);
