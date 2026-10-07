namespace Terraria.WorldStorage;

/// <summary>
/// Binds a TileEntity's persistent ID and tile-space anchor to its EntityRuntime root.
/// The anchor remains in tile coordinates and does not imply pixel-space state.
/// </summary>
public sealed record TileEntityBindingComponent(
  TileEntityId Id,
  TileEntityTypeId Type,
  TileCoordinate Anchor);
