namespace Terraria.Content;

public sealed record ItemStackDefinition(
  int MaxStack,
  bool UniqueStack,
  bool IsMaterial,
  int DefaultStack);
