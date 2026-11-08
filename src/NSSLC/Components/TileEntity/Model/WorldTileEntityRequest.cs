using Terraria.Communication;

namespace Terraria.World;

/// <summary>One cross-domain intention about a chest, a sign or a tile entity.</summary>
/// <remarks>
/// Communication raises these when a session asks to open a chest or read a sign; the World domain
/// owns the table and applies them. <see cref="Actor"/> carries the session so chest ownership and
/// sign edits stay attributable without the World domain reading session state.
/// </remarks>
public readonly record struct WorldTileEntityRequest(
    WorldTileEntityRequestKind Kind,
    WorldTileCoordinate Position,
    SessionEpoch Actor,
    string Text = "");
