using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public readonly record struct MechanismCooldownEntry(
    TileCoordinate Position,
    int RemainingTicks);
