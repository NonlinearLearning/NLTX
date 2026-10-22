using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Applies catalog-backed buff rules to the buff state owned by NpcManager.</summary>
public sealed class NpcBuffManager
{
  public const int TerrariaNpcBuffCapacity = 20;

  private readonly NpcManager _npcs;
  private readonly NpcBuffCatalog _catalog;

  public NpcBuffManager(NpcManager npcs, NpcBuffCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(catalog);
    _npcs = npcs;
    _catalog = catalog;
  }

  public NpcBuffChangeResult AddFromPacket(
    short npcSlot,
    CombatEntityIdentity expectedIdentity,
    ushort buffType,
    short buffTime)
  {
    if (buffType == 0 || buffTime <= 0)
    {
      return Reject(ProtocolErrorCode.InvalidLength, "Packet 53 requires a nonzero buff type and positive time.");
    }

    if (!_catalog.TryGetBuff(buffType, out NpcBuffDefinition definition))
    {
      return Reject(ProtocolErrorCode.NotReady, $"NPC buff type {buffType} has no migrated definition.");
    }

    if (!_npcs.TryCaptureBuffs(npcSlot, expectedIdentity, out NpcBuffView? current) || current is null)
    {
      return Reject(ProtocolErrorCode.StaleEntity, "Packet 53 targets a stale NPC slot or generation.");
    }

    if (!_catalog.TryGetImmunity(current.NpcType, out NpcBuffImmunityProfile profile))
    {
      return Reject(ProtocolErrorCode.NotReady, $"NPC type {current.NpcType} has no migrated buff immunity profile.");
    }

    if (profile.IsImmune(buffType))
    {
      return new NpcBuffChangeResult(OwnerCommitResult.Applied(current.CommitVersion), false, true, current);
    }

    var next = current.Buffs.ToList();
    int existingIndex = next.FindIndex(entry => entry.Type == buffType);
    if (existingIndex >= 0)
    {
      if (next[existingIndex].Time >= buffTime)
      {
        return new NpcBuffChangeResult(OwnerCommitResult.Applied(current.CommitVersion), false, true, current);
      }

      next[existingIndex] = new NpcBuffEntry(buffType, buffTime);
    }
    else
    {
      if (next.Count == TerrariaNpcBuffCapacity)
      {
        int removableIndex = FindFirstNonDebuff(next);
        if (removableIndex == -2)
        {
          return Reject(
            ProtocolErrorCode.NotReady,
            "An active NPC buff has no migrated definition, so slot replacement cannot be decided.");
        }

        if (removableIndex < 0)
        {
          return new NpcBuffChangeResult(OwnerCommitResult.Applied(current.CommitVersion), false, true, current);
        }

        next.RemoveAt(removableIndex);
      }

      next.Add(new NpcBuffEntry(buffType, buffTime));
    }

    (OwnerCommitResult commit, NpcBuffView? snapshot) = _npcs.CommitBuffs(current, next);
    bool applied = commit.Status == OwnerCommitStatus.Applied;
    return new NpcBuffChangeResult(commit, applied, applied, snapshot);
  }

  public NpcBuffChangeResult RequestRemoval(
    short npcSlot,
    CombatEntityIdentity expectedIdentity,
    ushort buffType)
  {
    if (buffType == 0)
    {
      return Reject(ProtocolErrorCode.InvalidLength, "Packet 137 requires a nonzero buff type.");
    }

    if (!_catalog.TryGetBuff(buffType, out NpcBuffDefinition definition))
    {
      return Reject(ProtocolErrorCode.NotReady, $"NPC buff type {buffType} has no migrated definition.");
    }

    if (!_npcs.TryCaptureBuffs(npcSlot, expectedIdentity, out NpcBuffView? current) || current is null)
    {
      return Reject(ProtocolErrorCode.StaleEntity, "Packet 137 targets a stale NPC slot or generation.");
    }

    if (!definition.CanBeRemovedByNetwork)
    {
      return new NpcBuffChangeResult(OwnerCommitResult.Applied(current.CommitVersion), false, false, current);
    }

    var next = current.Buffs.Where(entry => entry.Type != buffType).ToArray();
    if (next.Length == current.Buffs.Count)
    {
      return new NpcBuffChangeResult(OwnerCommitResult.Applied(current.CommitVersion), false, false, current);
    }

    (OwnerCommitResult commit, NpcBuffView? snapshot) = _npcs.CommitBuffs(current, next);
    bool applied = commit.Status == OwnerCommitStatus.Applied;
    return new NpcBuffChangeResult(commit, applied, applied, snapshot);
  }

  public NpcBuffChangeResult AddFromPacket(
    AddNPCBuffPacket packet,
    CombatEntityIdentity expectedIdentity)
  {
    ArgumentNullException.ThrowIfNull(packet);
    return AddFromPacket(packet.NpcIndex, expectedIdentity, packet.BuffType, packet.BuffTime);
  }

  public NpcBuffChangeResult RequestRemovalFromPacket(
    RequestNPCBuffRemovalPacket packet,
    CombatEntityIdentity expectedIdentity)
  {
    ArgumentNullException.ThrowIfNull(packet);
    return RequestRemoval(packet.NpcIndex, expectedIdentity, packet.BuffType);
  }

  public bool TryCapture(
    short npcSlot,
    CombatEntityIdentity expectedIdentity,
    out NpcBuffView? snapshot)
  {
    return _npcs.TryCaptureBuffs(npcSlot, expectedIdentity, out snapshot);
  }

  public IReadOnlyList<NpcBuffChangeResult> TickAll()
  {
    var results = new List<NpcBuffChangeResult>();
    foreach (NpcBuffView current in _npcs.CaptureAllBuffs())
    {
      if (current.Buffs.Count == 0 ||
          !_npcs.TryCaptureBuffs(current.NpcSlot, current.Entity, out NpcBuffView? latest) ||
          latest is null || latest.Buffs.Count == 0)
      {
        continue;
      }

      var next = new List<NpcBuffEntry>(latest.Buffs.Count);
      bool expired = false;
      foreach (NpcBuffEntry entry in latest.Buffs)
      {
        if (entry.Time > 1)
        {
          next.Add(entry with { Time = entry.Time - 1 });
        }
        else
        {
          expired = true;
        }
      }

      (OwnerCommitResult commit, NpcBuffView? snapshot) = _npcs.CommitBuffs(latest, next);
      bool applied = commit.Status == OwnerCommitStatus.Applied;
      results.Add(new NpcBuffChangeResult(commit, applied, applied && expired, snapshot));
    }

    return results.AsReadOnly();
  }

  private int FindFirstNonDebuff(IReadOnlyList<NpcBuffEntry> buffs)
  {
    for (int index = 0; index < buffs.Count; index++)
    {
      if (!_catalog.TryGetBuff(buffs[index].Type, out NpcBuffDefinition definition))
      {
        return -2;
      }

      if (!definition.IsDebuff)
      {
        return index;
      }
    }

    return -1;
  }

  private static NpcBuffChangeResult Reject(ProtocolErrorCode code, string detail)
  {
    return new NpcBuffChangeResult(OwnerCommitResult.Rejected(code, detail), false, false, null);
  }
}
