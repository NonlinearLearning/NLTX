using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Inventory.State;

internal static class Sc041ChestConcurrencyScenario
{
  public static string Run()
  {
    DateTimeOffset now = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
    var commitGate = new ServerStateCommitGate();
    var itemPolicy = new ItemValidationPolicy();
    var chests = new ChestStore(ContainerContentFacts.ChestIndexCount, itemPolicy, commitGate);
    var leases = new ChestLeaseStore(commitGate);
    var access = new AllowAllChestAccessPolicy();
    var service = new ChestAccessService(
      chests,
      leases,
      itemPolicy,
      access,
      TimeSpan.FromMinutes(2));

    var chestId = new ChestId(42);
    var worldScope = new WorldScopeId(7);
    OwnerCommitResult create = chests.Create(
      chestId,
      worldScope,
      new WorldTilePoint(10, 20),
      kindId: 0,
      name: "SC041",
      [new InventoryItemState(1, 10, 0, false), new InventoryItemState(2, 3, 0, false)],
      out ChestSnapshot? initialChest);
    Require(create.Status == OwnerCommitStatus.Applied && initialChest is not null,
      "SC041 could not create the authoritative chest fixture.");

    EntityUuid playerA = EntityUuid.Create();
    EntityUuid playerB = EntityUuid.Create();
    SessionEpoch epochA = SessionEpoch.Create(101);
    SessionEpoch epochB = SessionEpoch.Create(202);

    ChestAccessResult openA = service.TryOpen(
      chestId,
      playerA,
      epochA,
      worldScope,
      now);
    Require(openA.Commit.Status == OwnerCommitStatus.Applied && openA.Session is not null,
      "SC041 player A did not receive the first chest lease.");

    ChestAccessResult blockedB = service.TryOpen(
      chestId,
      playerB,
      epochB,
      worldScope,
      now.AddSeconds(1));
    Require(blockedB.Commit.Status == OwnerCommitStatus.Conflict,
      "SC041 player B unexpectedly acquired a concurrent chest lease.");

    ChestSnapshot leasedChest = openA.Chest!;
    ChestAccessResult updateA = service.TrySetSlot(
      chestId,
      playerA,
      epochA,
      worldScope,
      new PacketSequence(1),
      slotId: 0,
      new InventoryItemState(3, 4, 0, false),
      now.AddSeconds(2));
    Require(updateA.Commit.Status == OwnerCommitStatus.Applied,
      "SC041 player A slot update did not commit.");
    Require(updateA.Chest is not null && updateA.Chest.Revision.Value == 2,
      "SC041 slot update did not advance the authoritative chest revision.");
    Require(updateA.Chest!.Slots[0].Item == new InventoryItemState(3, 4, 0, false),
      "SC041 slot update did not publish the committed item.");

    int released = leases.ReleaseSession(epochA);
    Require(released == 1, "SC041 disconnect did not release player A's lease.");

    ChestAccessResult openBAfterDisconnect = service.TryOpen(
      chestId,
      playerB,
      epochB,
      worldScope,
      now.AddSeconds(3));
    Require(
      openBAfterDisconnect.Commit.Status == OwnerCommitStatus.Applied
      && openBAfterDisconnect.Session is not null,
      "SC041 player B did not acquire the lease after player A disconnected.");

    ChestAccessResult staleA = service.TrySetSlot(
      chestId,
      playerA,
      epochA,
      worldScope,
      new PacketSequence(2),
      slotId: 0,
      new InventoryItemState(4, 1, 0, false),
      now.AddSeconds(4));
    Require(staleA.Commit.Status == OwnerCommitStatus.Stale,
      "SC041 stale player A envelope was accepted after lease reuse.");

    Require(chests.TryCapture(chestId, out ChestSnapshot? capturedChest),
      "SC041 could not capture the final authoritative chest.");
    ChestSnapshot finalChest = capturedChest
      ?? throw new InvalidOperationException("SC041 final authoritative chest was null.");
    Require(finalChest.Generation == leasedChest.Generation,
      "SC041 changed chest generation during a lease-only session transition.");
    Require(finalChest.Revision.Value == 2,
      "SC041 stale write changed the chest revision.");
    Require(finalChest.Slots[0].Item == new InventoryItemState(3, 4, 0, false),
      "SC041 stale write changed the committed slot contents.");
    Require(finalChest.Slots[1].Item == new InventoryItemState(2, 3, 0, false),
      "SC041 unrelated chest slot was changed.");
    Require(leases.ReleaseSession(epochB) == 1,
      "SC041 final session cleanup did not release player B's lease.");

    return $"""
# SC041 result

Status: PASS

Fixture: authoritative chest index {chestId.Value}, world scope {worldScope.Value},
generation {finalChest.Generation.Value}, two item slots, and independent sessions
`{epochA.Value}` / `{epochB.Value}`.

Observed:

- Player A acquired the only writable lease.
- Concurrent player B was rejected with `Conflict`.
- A committed one slot update through the lease sequence `1`; revision advanced
  from `1` to `{finalChest.Revision.Value}` and the second slot remained unchanged.
- Releasing A's session removed exactly one lease; B then acquired a fresh lease.
- Replaying A's old session envelope was rejected with `Stale` and did not change
  the chest generation, revision, or item contents.
- Final B-session cleanup removed the remaining lease.

This proves the selected SC041 container concurrency, one-slot commit, stale
session rejection, and lease cleanup path. It does not claim SC042–SC055 or the
full server registration/transport path was run.
""";
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class AllowAllChestAccessPolicy : IChestAccessPolicy
  {
    public bool CanOpen(EntityUuid playerId, WorldScopeId worldScope, ChestSnapshot chest)
    {
      return playerId.IsValid && worldScope.IsValid && chest.WorldScope == worldScope;
    }
  }
}
