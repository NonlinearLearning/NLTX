namespace Terraria.Share.Items;

/// <summary>Read-only view of authoritative world items for other domains.</summary>
/// <remarks>
/// Reads are allowed across domains — a caller may inspect an item to decide what to request.
/// Writes are not: changes go through <see cref="ItemRequest"/>.
/// </remarks>
public interface IItemQuery
{
  bool TryGet(short slot, out ItemState? item);
}
