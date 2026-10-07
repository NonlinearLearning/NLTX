using System.Collections.ObjectModel;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Rebuildable section membership index. It does not own global section activity or persistence.
/// </summary>
public sealed class LeashedSectionIndex
{
  private sealed class Bucket
  {
    public Bucket(SectionCoordinate section)
    {
      Section = section;
    }

    public SectionCoordinate Section { get; }

    public bool IsActive { get; set; }

    public List<LeashedEntityHandle?> Handles { get; } = new();

    public int LogicalCount { get; set; }

    public int EmptySlots { get; set; }
  }

  private readonly Dictionary<SectionCoordinate, Bucket> _buckets = new();

  public int SectionCount => _buckets.Count;

  internal void SetActive(SectionCoordinate section, bool active)
  {
    Bucket bucket = GetOrCreate(section);
    bucket.IsActive = active;
  }

  internal int Add(SectionCoordinate section, LeashedEntityHandle handle)
  {
    if (!handle.IsAssigned)
    {
      throw new ArgumentException("An assigned entity handle is required.", nameof(handle));
    }

    Bucket bucket = GetOrCreate(section);
    int slot = bucket.EmptySlots > 0
      ? ReuseEmptySlot(bucket, handle)
      : Append(bucket, handle);

    return slot;
  }

  internal bool Remove(
    SectionCoordinate section,
    LeashedEntityHandle handle,
    int sectionSlot)
  {
    if (!_buckets.TryGetValue(section, out Bucket? bucket)
      || sectionSlot < 0
      || sectionSlot >= bucket.LogicalCount
      || bucket.Handles[sectionSlot] != handle)
    {
      return false;
    }

    bucket.Handles[sectionSlot] = null;
    bucket.EmptySlots++;
    return true;
  }

  public bool Contains(
    SectionCoordinate section,
    LeashedEntityHandle handle,
    int sectionSlot)
  {
    return _buckets.TryGetValue(section, out Bucket? bucket)
      && sectionSlot >= 0
      && sectionSlot < bucket.LogicalCount
      && bucket.Handles[sectionSlot] == handle;
  }

  internal IReadOnlyList<LeashedSectionSlotChange> CompactIfNecessary(
    SectionCoordinate section)
  {
    if (!_buckets.TryGetValue(section, out Bucket? bucket)
      || bucket.EmptySlots < bucket.LogicalCount / 2)
    {
      return Array.Empty<LeashedSectionSlotChange>();
    }

    List<LeashedSectionSlotChange> changes = new();
    int compactedCount = 0;
    for (int previousSlot = 0; previousSlot < bucket.LogicalCount; previousSlot++)
    {
      LeashedEntityHandle? handle = bucket.Handles[previousSlot];
      if (!handle.HasValue)
      {
        continue;
      }

      if (previousSlot != compactedCount)
      {
        changes.Add(
          new LeashedSectionSlotChange(handle.Value, previousSlot, compactedCount));
      }

      bucket.Handles[compactedCount++] = handle;
    }

    bucket.Handles.RemoveRange(compactedCount, bucket.Handles.Count - compactedCount);
    bucket.LogicalCount = compactedCount;
    bucket.EmptySlots = 0;
    return new ReadOnlyCollection<LeashedSectionSlotChange>(changes);
  }

  public bool TryGetSnapshot(
    SectionCoordinate section,
    out LeashedSectionSnapshot snapshot)
  {
    if (!_buckets.TryGetValue(section, out Bucket? bucket))
    {
      snapshot = null!;
      return false;
    }

    List<LeashedEntityHandle> handles = new();
    for (int i = 0; i < bucket.LogicalCount; i++)
    {
      LeashedEntityHandle? handle = bucket.Handles[i];
      if (handle.HasValue)
      {
        handles.Add(handle.Value);
      }
    }

    snapshot = new LeashedSectionSnapshot(
      bucket.Section,
      bucket.IsActive,
      handles,
      bucket.EmptySlots);
    return true;
  }

  public IReadOnlyList<LeashedEntityHandle> GetHandles(SectionCoordinate section)
  {
    if (!TryGetSnapshot(section, out LeashedSectionSnapshot snapshot))
    {
      return Array.Empty<LeashedEntityHandle>();
    }

    return snapshot.Handles;
  }

  internal void Clear()
  {
    _buckets.Clear();
  }

  private static int Append(Bucket bucket, LeashedEntityHandle handle)
  {
    bucket.Handles.Add(handle);
    bucket.LogicalCount++;
    return bucket.LogicalCount - 1;
  }

  private static int ReuseEmptySlot(Bucket bucket, LeashedEntityHandle handle)
  {
    for (int i = 0; i < bucket.LogicalCount; i++)
    {
      if (bucket.Handles[i].HasValue)
      {
        continue;
      }

      bucket.Handles[i] = handle;
      bucket.EmptySlots--;
      return i;
    }

    throw new InvalidOperationException("The section index has no reusable slot.");
  }

  private Bucket GetOrCreate(SectionCoordinate section)
  {
    if (_buckets.TryGetValue(section, out Bucket? bucket))
    {
      return bucket;
    }

    bucket = new Bucket(section);
    _buckets.Add(section, bucket);
    return bucket;
  }
}
