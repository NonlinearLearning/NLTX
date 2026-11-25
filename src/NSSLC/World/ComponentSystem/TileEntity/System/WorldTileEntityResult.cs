using Terraria.Unshared.Session;

namespace Terraria.Unshared.World;

/// <summary>Result of one tile entity intention, published for the requesting session.</summary>
public readonly record struct WorldTileEntityResult(
    WorldTileEntityRequestKind Kind,
    WorldTileEntityResultKind Outcome,
    WorldTileCoordinate Position,
    SessionEpoch Actor);
