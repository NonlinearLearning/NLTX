namespace Terraria.WorldStorage;

public sealed class WorldSignStore
{
  private WorldSignState?[] _signs = Array.Empty<WorldSignState?>();
  private Dictionary<TileCoordinate, SignSlot> _signSlotsByAnchor = new();
  private int _activeSignCount;
  private long _mutationRevision;

  public int Capacity => _signs.Length;
  public int ActiveSignCount => _activeSignCount;
  public long MutationRevision => _mutationRevision;

  public IReadOnlyList<WorldSignSnapshot> CreateSnapshot() {
    return Array.AsReadOnly(_signs.Where(sign => sign is not null)
        .Select(sign => new WorldSignSnapshot(sign!.Anchor, sign.Text)).ToArray());
  }

  internal void Replace(IReadOnlyList<WorldSignSnapshot> snapshots) {
    var signs = new WorldSignState?[snapshots.Count];
    var anchors = new Dictionary<TileCoordinate, SignSlot>();
    for (int i = 0; i < snapshots.Count; i++) {
      var slot = new SignSlot(i);
      anchors.Add(snapshots[i].Anchor, slot);
      signs[i] = new WorldSignState {
        Slot = slot, Anchor = snapshots[i].Anchor, Text = snapshots[i].Text
      };
    }
    _signs = signs;
    _signSlotsByAnchor = anchors;
    _activeSignCount = signs.Length;
    _mutationRevision++;
  }
}
