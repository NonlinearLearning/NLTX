using System;

namespace Terraria.Npc.Queries;

public sealed class NpcActivePresenceCache
{
  private readonly bool[] _activeByNpcType;

  public NpcActivePresenceCache(int npcTypeCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(npcTypeCount);
    _activeByNpcType = new bool[npcTypeCount];
    CurrentScanRevision = -1;
  }

  public long CurrentScanRevision { get; private set; }

  public bool IsValid { get; private set; }

  public void BeginScan(long scanRevision)
  {
    if (scanRevision <= CurrentScanRevision)
    {
      throw new ArgumentOutOfRangeException(
        nameof(scanRevision),
        "A scan revision must increase for each active-presence scan.");
    }

    Array.Clear(_activeByNpcType);
    CurrentScanRevision = scanRevision;
    IsValid = true;
  }

  public bool TryMarkActive(long scanRevision, int npcType)
  {
    if (!IsCurrentScan(scanRevision) || !IsValidNpcType(npcType))
    {
      return false;
    }

    _activeByNpcType[npcType] = true;
    return true;
  }

  public bool TryGetActive(long scanRevision, int npcType, out bool isActive)
  {
    if (!IsCurrentScan(scanRevision) || !IsValidNpcType(npcType))
    {
      isActive = false;
      return false;
    }

    isActive = _activeByNpcType[npcType];
    return true;
  }

  public bool TryGetActiveNpcTypes(long scanRevision, out int[] activeNpcTypes)
  {
    if (!IsCurrentScan(scanRevision))
    {
      activeNpcTypes = Array.Empty<int>();
      return false;
    }

    var activeCount = 0;
    for (var i = 0; i < _activeByNpcType.Length; i++)
    {
      if (_activeByNpcType[i])
      {
        activeCount++;
      }
    }

    activeNpcTypes = new int[activeCount];
    var resultIndex = 0;
    for (var i = 0; i < _activeByNpcType.Length; i++)
    {
      if (_activeByNpcType[i])
      {
        activeNpcTypes[resultIndex++] = i;
      }
    }

    return true;
  }

  public void Invalidate()
  {
    Array.Clear(_activeByNpcType);
    IsValid = false;
  }

  private bool IsCurrentScan(long scanRevision)
  {
    return IsValid && scanRevision == CurrentScanRevision;
  }

  private bool IsValidNpcType(int npcType)
  {
    return (uint)npcType < (uint)_activeByNpcType.Length;
  }
}
