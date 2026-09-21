using System;
using System.Collections.Generic;
using Terraria.Npc;
using Terraria.Npc.Environment;
using Terraria.Npc.Network;
using Terraria.Npc.Queries;
using Terraria.Server.Npc;
using Terraria.Town.Doors;
using Terraria.Town.Housing;
using Terraria.Town.Progression.Pets;
using Terraria.Town.Progression.Rescue;
using Terraria.Town.Progression.Spawn;
using Terraria.Town.Residents;
using Terraria.Town;
using Terraria.WorldSession.NpcProgression.Boss;
using Terraria.WorldSession.NpcProgression.Books;
using Terraria.WorldSession.NpcProgression.Events;
using Terraria.WorldSession.NpcProgression.Invasion;
using Terraria.WorldSession.NpcProgression.LunarTower;
using Terraria.WorldSession.NpcProgression.MoonLord;
using Terraria.WorldSession.Town;

static class Program
{
  private static int Main()
  {
    try
    {
      VerifyMoonLordAndInvasion();
      VerifyBossRegistryAndNetworkIntent();
      VerifyTownProgression();
      VerifyDefeatAndTowerProgression();
      VerifyActivePresenceAndTownState();
      VerifyHousingDoorAndBreathState();
      Console.WriteLine("PASS: P13 NPC/Town progression focused verifier (105 implemented members; nextDialogue deferred)");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void VerifyMoonLordAndInvasion()
  {
    int[,,,] attacks = new int[1, 1, 1, 1];
    attacks[0, 0, 0, 0] = 17;
    int[,] attacksTwo = new int[1, 1];
    attacksTwo[0, 0] = 23;
    MoonLordEncounterDefinition definition = new(
      attacks,
      attacksTwo,
      moonLordFightingDistance: 4500,
      maxMoonLordCountdown: 3600,
      naturalMoonlordCountdownTime: 3600,
      itemMoonlordCountdownTime: 720);

    Require(definition.MoonLordFightingDistance == 4500, "C01 fighting distance");
    Require(definition.MaxMoonLordCountdown == 3600, "C01 maximum countdown");
    Require(definition.NaturalMoonlordCountdownTime == 3600, "C01 natural countdown");
    Require(definition.ItemMoonlordCountdownTime == 720, "C01 item countdown");
    Require(definition.GetMoonLordAttacksArray()[0, 0, 0, 0] == 17, "C01 attack table");
    Require(definition.GetMoonLordAttacksArray2()[0, 0] == 23, "C01 secondary attack table");

    MoonLordEncounterStateComponent moonLordState = new(3600);
    moonLordState.SetCountdown(720);
    Require(moonLordState.MoonLordCountdown == 720, "C01 mutable countdown");
    RequireThrows<ArgumentOutOfRangeException>(
      () => moonLordState.SetCountdown(3601),
      "C01 countdown upper bound");
    moonLordState.Reset();
    Require(moonLordState.MoonLordCountdown == 0, "C01 reset");

    InvasionWaveProgressStateComponent invasionState = new();
    invasionState.Commit(15.5f, 4.5f, 2);
    Require(invasionState.TotalInvasionPoints == 15.5f, "C02 invasion points");
    Require(invasionState.WaveKills == 4.5f, "C02 wave kills");
    Require(invasionState.WaveNumber == 2, "C02 wave number");
    RequireThrows<ArgumentOutOfRangeException>(
      () => invasionState.Commit(15.5f, 4.5f, 1),
      "C02 wave cannot move backwards");
    invasionState.Reset();
    Require(invasionState.WaveNumber == 0 && invasionState.TotalInvasionPoints == 0.0f,
      "C02 reset");
  }

  private static void VerifyBossRegistryAndNetworkIntent()
  {
    BossEntityIndexRegistryComponent registry = new();
    BossEntityIndexKey golem = new(
      new NpcInstanceId(1),
      new NpcSlot(10),
      Generation: 1,
      new NpcTypeId(245));
    BossEntityIndexKey plant = new(
      new NpcInstanceId(2),
      new NpcSlot(11),
      Generation: 1,
      new NpcTypeId(262));
    BossEntityIndexKey crimson = new(
      new NpcInstanceId(3),
      new NpcSlot(12),
      Generation: 1,
      new NpcTypeId(266));
    BossEntityIndexKey deerclops = new(
      new NpcInstanceId(4),
      new NpcSlot(13),
      Generation: 1,
      new NpcTypeId(668));

    Require(registry.TryRegister(BossKind.Golem, golem), "C03 golem registration");
    Require(registry.TryRegister(BossKind.Plant, plant), "C03 plant registration");
    Require(registry.TryRegister(BossKind.Crimson, crimson), "C03 crimson registration");
    Require(registry.TryRegister(BossKind.Deerclops, deerclops), "C03 deerclops registration");
    Require(registry.GolemBoss == 10 && registry.PlantBoss == 11, "C03 legacy slots");
    Require(registry.CrimsonBoss == 12 && registry.DeerclopsBoss == 13, "C03 legacy slots 2");
    Require(!registry.TryRegister(BossKind.Golem, golem), "C03 duplicate registration");
    Require(
      registry.TryResolve(BossKind.Golem, golem, isActive: true, out BossEntityIndexKey resolved)
      && resolved == golem,
      "C03 active resolution");
    Require(!registry.TryResolve(BossKind.Golem, golem, isActive: false, out _),
      "C03 inactive resolution");
    Require(registry.TryClear(BossKind.Golem, golem), "C03 current clear");
    Require(registry.GolemBoss == -1, "C03 cleared legacy slot");

    NpcNetworkSyncIntentComponent intent = new();
    Require(intent.Mark() && intent.IsPending && intent.Revision == 1, "C04 first mark");
    Require(!intent.Mark(), "C04 duplicate mark coalescing");
    Require(!intent.Acknowledge(2) && intent.IsPending, "C04 stale acknowledgement");
    Require(intent.Acknowledge(1) && !intent.IsPending, "C04 acknowledgement");

    NpcReplicationDirtyState dirtyState = new();
    dirtyState.Flags = NpcReplicationFlags.SpawnNeedsSync;
    Require(dirtyState.RequiresSpawnSync && !dirtyState.IsDirty, "C04 compatibility flags");
    dirtyState.MarkStateChanged();
    Require(dirtyState.IsDirty, "C04 dirty state owner");
    dirtyState.ResetForEntityReuse();
    Require(!dirtyState.IsDirty && !dirtyState.RequiresSpawnSync, "C04 reset");
  }

  private static void VerifyTownProgression()
  {
    TownRescueProgressStateComponent rescue = new();
    foreach (TownRescueKind kind in Enum.GetValues<TownRescueKind>())
    {
      Require(rescue.MarkRescued(kind), $"C05 rescue {kind}");
      Require(rescue.IsRescued(kind), $"C05 rescue readback {kind}");
      Require(!rescue.MarkRescued(kind), $"C05 duplicate rescue {kind}");
    }
    rescue.Reset();
    Require(!rescue.SavedTaxCollector && !rescue.SavedGolfer, "C05 reset");

    TownPetAdoptionProgressStateComponent pets = new();
    foreach (TownPetKind kind in Enum.GetValues<TownPetKind>())
    {
      Require(pets.Adopt(kind), $"C06 adoption {kind}");
      Require(pets.IsAdopted(kind), $"C06 adoption readback {kind}");
      Require(!pets.Adopt(kind), $"C06 duplicate adoption {kind}");
    }
    pets.Reset();
    Require(!pets.BoughtCat && !pets.BoughtDog && !pets.BoughtBunny, "C06 reset");

    TownSpawnUnlockStateComponent unlocks = new();
    foreach (TownSpawnUnlockKind kind in Enum.GetValues<TownSpawnUnlockKind>())
    {
      Require(unlocks.Unlock(kind), $"C07 unlock {kind}");
      Require(unlocks.IsUnlocked(kind), $"C07 unlock readback {kind}");
      Require(!unlocks.Unlock(kind), $"C07 duplicate unlock {kind}");
    }
    unlocks.Reset();
    Require(!unlocks.PrincessUnlocked && !unlocks.SlimeBlueUnlocked, "C07 reset");

    NpcProgressionBookUsageStateComponent books = new();
    foreach (NpcProgressionBookKind kind in Enum.GetValues<NpcProgressionBookKind>())
    {
      Require(books.MarkUsed(kind), $"C08 book use {kind}");
      Require(books.IsUsed(kind), $"C08 book readback {kind}");
      Require(!books.MarkUsed(kind), $"C08 duplicate book use {kind}");
    }
    books.Reset();
    Require(!books.CombatBookWasUsed && !books.PeddlersSatchelWasUsed, "C08 reset");
  }

  private static void VerifyDefeatAndTowerProgression()
  {
    BossDefeatProgressionStateComponent bosses = new();
    foreach (BossDefeatProgressionKind kind in Enum.GetValues<BossDefeatProgressionKind>())
    {
      if (kind == BossDefeatProgressionKind.MechBossAny && bosses.IsDefeated(kind))
      {
        continue;
      }

      Require(bosses.MarkDefeated(kind), $"C11 defeat {kind}");
      Require(bosses.IsDefeated(kind), $"C11 defeat readback {kind}");
    }
    Require(bosses.DownedMechBossAny, "C11 mech compatibility aggregate");
    bosses.Reset();
    Require(!bosses.DownedMoonlord && !bosses.DownedMechBossAny, "C11 reset");

    EventDefeatProgressionStateComponent events = new();
    foreach (EventDefeatProgressionKind kind in Enum.GetValues<EventDefeatProgressionKind>())
    {
      Require(events.MarkDefeated(kind), $"C12 defeat {kind}");
      Require(events.IsDefeated(kind), $"C12 defeat readback {kind}");
    }
    events.Reset();
    Require(!events.DownedGoblins && !events.DownedTowerStardust, "C12 reset");

    LunarTowerEncounterStateComponent towers = new();
    foreach (LunarTowerKind kind in Enum.GetValues<LunarTowerKind>())
    {
      towers.SetShieldStrength(kind, 100);
      towers.SetTowerActive(kind, true);
      Require(towers.GetShieldStrength(kind) == 100, $"C10 shield readback {kind}");
      Require(towers.IsTowerActive(kind), $"C10 active readback {kind}");
      Require(towers.ApplyShieldDamage(kind, 25) == 75, $"C10 shield damage {kind}");
    }
    towers.SetApocalypseActive(true);
    Require(towers.LunarApocalypseIsUp, "C10 apocalypse state");
    towers.Reset();
    Require(!towers.LunarApocalypseIsUp && towers.GetShieldStrength(LunarTowerKind.Solar) == 0,
      "C10 reset");
  }

  private static void VerifyActivePresenceAndTownState()
  {
    NpcActivePresenceCache cache = new(700);
    NpcActivePresenceScanSystem.Rebuild(
      cache,
      10,
      [
        new NpcActivePresenceScanEntry(668, true),
        new NpcActivePresenceScanEntry(245, true),
        new NpcActivePresenceScanEntry(245, true),
        new NpcActivePresenceScanEntry(312, false),
        new NpcActivePresenceScanEntry(-1, true)
      ]);
    Require(cache.TryGetActiveNpcTypes(10, out int[] activeTypes), "C09 scan snapshot");
    Require(activeTypes.AsSpan().SequenceEqual(new[] { 245, 668 }), "C09 scan types");
    NpcActivePresenceScanSystem.Rebuild(
      cache,
      11,
      [new NpcActivePresenceScanEntry(310, true)]);
    Require(!cache.TryGetActive(10, 245, out _), "C09 stale scan");
    Require(cache.TryGetActive(11, 310, out bool isActive) && isActive, "C09 current scan");

    TownResidentStateComponent resident = new();
    resident.SetResident(true);
    Require(resident.IsTownResident, "C13 resident state");
    resident.Reset();
    Require(!resident.IsTownResident, "C13 reset");

    TravelNpcWorldStateComponent travel = new();
    travel.SetTravelNpcActive(true);
    Require(travel.IsTravelNpcActive, "C14 travel state");
    travel.Reset();
    Require(!travel.IsTravelNpcActive, "C14 reset");
  }

  private static void VerifyHousingDoorAndBreathState()
  {
    TownHousingRelationStateComponent housing = new();
    TownRoomTilePoint home = new(12, 34);
    housing.CommitRelation(false, false, 7, home, 2);
    Require(housing.HasHome && housing.HomeTile == home && housing.HousingCategory == 2,
      "C15 assigned housing");
    housing.CommitRelation(true, true, TownHousingRuleDefinition.KickOutLookForHomeTimeout, null, 0);
    Require(housing.IsHomeless && housing.HomelessDespawn && housing.OldHomeTile == home,
      "C15 homeless transition and snapshot");
    RequireThrows<ArgumentException>(
      () => housing.CommitRelation(true, false, 0, home, 0),
      "C15 homeless home invariant");
    housing.Reset();
    Require(!housing.IsHomeless && housing.HomeTile is null, "C15 reset");

    TownDoorInteractionIntentComponent doors = new();
    TownDoorInteractionIntent intent = doors.Request(true, 4, 5, expiresAtTick: 20);
    Require(doors.HasPendingIntent && intent.Sequence == 1 && doors.CloseDoor, "C16 request");
    Require(doors.TryConsume(10, out TownDoorInteractionIntent consumed) && consumed == intent,
      "C16 consume once");
    Require(!doors.TryConsume(10, out _), "C16 duplicate consume");
    doors.Request(false, 6, 7, expiresAtTick: 10);
    Require(!doors.TryConsume(10, out _), "C16 expiry");
    doors.Reset();
    Require(!doors.HasPendingIntent, "C16 reset");

    NpcBreathStateComponent breath = new(1, 0);
    for (var step = 0; step < NpcBreathRuleDefinition.DrowningCadence - 1; step++)
    {
      Require(!breath.ApplyEnvironmentStep(true), "C17 cadence before zero");
    }
    Require(breath.ApplyEnvironmentStep(true) && breath.Breath == 0, "C17 zero transition");
    breath.ApplyEnvironmentStep(false);
    Require(breath.Breath == NpcBreathRuleDefinition.BreathRecoveryPerEnvironmentStep,
      "C17 recovery");
    breath.Reset();
    Require(breath.Breath == NpcBreathRuleDefinition.BreathMax && breath.BreathCounter == 0,
      "C17 reset");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action.Invoke();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
