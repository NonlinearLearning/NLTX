using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Npc;

internal sealed class RecordingNpcSpawnSlotAcquisitionPort : INpcSpawnPreCommitPort
{
  private readonly Queue<int> _randomValues;
  private readonly Queue<int> _luckValues;
  private readonly bool[] _activeSlots;
  private readonly int[] _protectionTicks;
  private readonly NpcSpawnSlotFact[] _slotFacts;
  private readonly bool _searchInReverse;
  private readonly bool _cannotSpawnInSlot0;

  public RecordingNpcSpawnSlotAcquisitionPort(
    NpcSpawnSlotFact[] slotFacts,
    IEnumerable<int> randomValues,
    bool searchInReverse,
    bool cannotSpawnInSlot0,
    IEnumerable<int>? luckValues = null)
  {
    _slotFacts = slotFacts;
    _randomValues = new Queue<int>(randomValues);
    _luckValues = new Queue<int>(luckValues ?? Array.Empty<int>());
    _activeSlots = slotFacts.Select(fact => fact.IsActive).ToArray();
    _protectionTicks = slotFacts.Select(fact => fact.SpawnSlotProtection).ToArray();
    _searchInReverse = searchInReverse;
    _cannotSpawnInSlot0 = cannotSpawnInSlot0;
  }

  public List<string> Calls { get; } = new();

  public int SlotCount => _slotFacts.Length;

  public int Next(int exclusiveUpperBound)
  {
    Calls.Add($"random:{exclusiveUpperBound}");
    if (!_randomValues.TryDequeue(out int value))
    {
      throw new InvalidOperationException("No recorded slot-acquisition random value remains.");
    }

    if ((uint)value >= (uint)exclusiveUpperBound)
    {
      throw new InvalidOperationException("The recorded random value is outside its requested range.");
    }

    return value;
  }

  public NpcTypeId FromNetId(NpcTypeId resolvedType)
  {
    Calls.Add($"from-net-id:{resolvedType.Value}");
    return resolvedType;
  }

  public int RollLuck(int range)
  {
    Calls.Add($"roll-luck:{range}");
    if (!_luckValues.TryDequeue(out int value))
    {
      throw new InvalidOperationException("No recorded luck value remains.");
    }

    if ((uint)value >= (uint)range)
    {
      throw new InvalidOperationException("The recorded luck value is outside its requested range.");
    }

    return value;
  }

  public bool SearchSpawnSlotsInReverse(NpcTypeId netType)
  {
    Calls.Add($"reverse:{netType.Value}");
    return _searchInReverse;
  }

  public bool CannotSpawnInSlot0(NpcTypeId netType)
  {
    Calls.Add($"slot-zero:{netType.Value}");
    return _cannotSpawnInSlot0;
  }

  public NpcSpawnSlotFact[] CaptureSlotFacts()
  {
    Calls.Add("capture-slot-facts");
    return (NpcSpawnSlotFact[])_slotFacts.Clone();
  }

  public bool IsNpcActive(int slotIndex)
  {
    Calls.Add($"active:{slotIndex}");
    return _activeSlots[slotIndex];
  }

  public int ReadProtectionTicks(int slotIndex)
  {
    Calls.Add($"read:{slotIndex}");
    return _protectionTicks[slotIndex];
  }

  public void WriteProtectionTicks(int slotIndex, int protectionTicks)
  {
    Calls.Add($"write:{slotIndex}:{protectionTicks}");
    _protectionTicks[slotIndex] = protectionTicks;
  }
}
