using System.Numerics;
using EntityEcs;
using NSSLC.WorldGeneration.Utilities;
using Terraria.Content;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Npc;
using Terraria.Relationships;
using RuntimeMain = NSSLC.WorldGeneration.Main;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class NpcDeathAuthorityProbe
{
  internal static object Run(
    RuntimeNpcStore npcs,
    LoadedWorldSession session,
    ContentCatalog catalog)
  {
    int originalNetMode = RuntimeMain.netMode;
    try
    {
      object singlePlayer = VerifyAuthorityMode(
        npcs,
        session,
        catalog,
        netMode: 0,
        tickNumber: 700,
        randomSeed: 0x53494E47);
      object server = VerifyAuthorityMode(
        npcs,
        session,
        catalog,
        netMode: 2,
        tickNumber: 701,
        randomSeed: 0x53455256);
      object client = VerifyAuthorityMode(
        npcs,
        session,
        catalog,
        netMode: 1,
        tickNumber: 702,
        randomSeed: 0x434C4945);
      return new
      {
        Passed = true,
        SinglePlayer = singlePlayer,
        Server = server,
        Client = client,
      };
    }
    finally
    {
      RuntimeMain.netMode = originalNetMode;
    }
  }

  private static object VerifyAuthorityMode(
    RuntimeNpcStore npcs,
    LoadedWorldSession session,
    ContentCatalog catalog,
    int netMode,
    long tickNumber,
    int randomSeed)
  {
    RuntimeMain.netMode = netMode;
    RuntimeMain.rand = new UnifiedRandom(randomSeed);
    int worldId = session.World.Descriptor.WorldId;
    Vector2 position = new(
      session.World.Descriptor.SpawnTileX * 16f,
      session.World.Descriptor.SpawnTileY * 16f - 40f);
    if (!npcs.TrySpawnParentChild(
          SimulationContentSupportManifest.GreenSlimeNetId,
          SimulationContentSupportManifest.BlueSlimeNetId,
          position,
          tickNumber,
          catalog,
          worldId,
          out RuntimeNpcStore.NpcParentRelationBinding binding))
    {
      throw new InvalidOperationException(
        $"Could not create a Mother Slime life owner for netMode {netMode}.");
    }

    if (!npcs.TryGetAt(binding.ParentSlot, out RuntimeNpcEntity? parent) || parent is null ||
        !npcs.TryGetAt(binding.ChildSlot, out RuntimeNpcEntity? child) || child is null)
    {
      throw new InvalidOperationException(
        "The death authority probe could not resolve the NPC pair.");
    }

    HashSet<RuntimeEntityHandle> existingHandles = npcs.CreateProjectileTargetSnapshot()
      .Select(target => target.RuntimeHandle)
      .ToHashSet();
    RuntimeNpcProjectileTargetSnapshot target = npcs.CreateProjectileTargetSnapshot()
      .Single(candidate => candidate.RuntimeHandle == child.RuntimeHandle);
    npcs.AdvanceDamageTrackingTo(tickNumber);
    if (!npcs.TryApplyProjectileHit(
          target,
          damage: 100_000,
          ownerSlot: 0,
          tickNumber,
          knockback: 0f,
          hitDirection: 1,
          out NpcStrikeResult strike,
          out _))
    {
      throw new InvalidOperationException(
        $"Could not apply the lethal child hit for netMode {netMode}.");
    }

    RuntimeNpcProjectileTargetSnapshot[] spawned = npcs.CreateProjectileTargetSnapshot()
      .Where(candidate => !existingHandles.Contains(candidate.RuntimeHandle))
      .ToArray();
    if (!strike.CombatResult.DeathTransitioned ||
        npcs.TryResolveEntityReference(binding.ParentReference, out _) ||
        npcs.TryResolveEntityReference(binding.ChildReference, out _))
    {
      throw new InvalidOperationException(
        $"The Mother Slime life owner and child did not terminate for netMode {netMode}.");
    }

    if (netMode == 1)
    {
      if (spawned.Length != 0)
      {
        throw new InvalidOperationException("A client created Mother Slime split children.");
      }
    }
    else
    {
      if (spawned.Length is < 2 or > 3 ||
          spawned.Any(candidate =>
            candidate.NetId != SimulationContentSupportManifest.BlueSlimeNetId))
      {
        throw new InvalidOperationException(
          $"SinglePlayer/Server created an invalid split child count for netMode {netMode}.");
      }

      if (netMode == 2)
      {
        foreach (RuntimeNpcProjectileTargetSnapshot childTarget in spawned)
        {
          if (!npcs.TryGetAt(childTarget.Slot.Value, out RuntimeNpcEntity? splitChild) ||
              splitChild is null ||
              !splitChild.CaptureImmediateEffects().NetworkUpdateRequested)
          {
            throw new InvalidOperationException(
              "A server-created split child did not request synchronization.");
          }
        }
      }
    }

    return new
    {
      NetMode = netMode,
      DeathTransitioned = strike.CombatResult.DeathTransitioned,
      LifeOwnerReleased = !npcs.TryResolveEntityReference(binding.ParentReference, out _),
      AttachedChildReleased = !npcs.TryResolveEntityReference(binding.ChildReference, out _),
      SplitChildrenCreated = spawned.Length,
      ServerChildrenRequestSync = netMode != 2 || spawned.All(candidate =>
        npcs.TryGetAt(candidate.Slot.Value, out RuntimeNpcEntity? splitChild) &&
        splitChild is not null &&
        splitChild.CaptureImmediateEffects().NetworkUpdateRequested),
    };
  }
}
