using System;
using System.Collections.Generic;
using Terraria.Npc.Queries;

static class Program
{
  private static int Main()
  {
    try
    {
      TestActivePresenceRebuild();
      TestRevisionReplacementAndStaleReads();
      TestInvalidationAndRevisionOrdering();
      Console.WriteLine("PASS: NpcActivePresenceScanSystem C09 focused verifier");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestActivePresenceRebuild()
  {
    NpcActivePresenceCache cache = new(npcTypeCount: 700);

    List<NpcActivePresenceScanEntry> entries =
    [
      new(668, true),
      new(245, true),
      new(312, false),
      new(245, true),
      new(-1, true),
      new(700, true)
    ];

    NpcActivePresenceScanSystem.Rebuild(cache, scanRevision: 10, entries);

    Require(cache.IsValid, "revision 10 must be valid");
    Require(cache.CurrentScanRevision == 10, "revision 10 must be recorded");
    Require(cache.TryGetActive(10, 245, out bool golemActive) && golemActive,
      "active type 245 must be recorded");
    Require(cache.TryGetActive(10, 668, out bool deerclopsActive) && deerclopsActive,
      "active type 668 must be recorded");
    Require(cache.TryGetActive(10, 312, out bool inactiveType) && !inactiveType,
      "inactive entries must not be recorded");
    Require(!cache.TryGetActive(10, -1, out _), "negative type must be rejected");
    Require(!cache.TryGetActive(10, 700, out _), "capacity boundary type must be rejected");

    Require(
      cache.TryGetActiveNpcTypes(10, out int[] activeTypes),
      "current revision snapshot must be available");
    Require(
      activeTypes.AsSpan().SequenceEqual(new[] { 245, 668 }),
      "snapshot must be ascending and deduplicated");
  }

  private static void TestRevisionReplacementAndStaleReads()
  {
    NpcActivePresenceCache cache = new(npcTypeCount: 700);
    NpcActivePresenceScanSystem.Rebuild(
      cache,
      scanRevision: 10,
      entries: [new NpcActivePresenceScanEntry(245, true), new NpcActivePresenceScanEntry(668, true)]);

    NpcActivePresenceScanSystem.Rebuild(
      cache,
      scanRevision: 11,
      entries: [new NpcActivePresenceScanEntry(245, false), new NpcActivePresenceScanEntry(310, true)]);

    Require(cache.CurrentScanRevision == 11, "revision 11 must replace revision 10");
    Require(!cache.TryGetActive(10, 245, out _), "revision 10 point reads must be stale");
    Require(cache.TryGetActive(11, 245, out bool oldType) && !oldType,
      "inactive revision 11 entry must remain false");
    Require(cache.TryGetActive(11, 310, out bool newType) && newType,
      "revision 11 active type must be recorded");
    Require(cache.TryGetActive(11, 668, out bool clearedType) && !clearedType,
      "revision 10 types must be cleared before revision 11");
    Require(
      cache.TryGetActiveNpcTypes(11, out int[] activeTypes),
      "revision 11 snapshot must be available");
    Require(
      activeTypes.AsSpan().SequenceEqual(new[] { 310 }),
      "revision 11 snapshot must contain only current active types");
  }

  private static void TestInvalidationAndRevisionOrdering()
  {
    NpcActivePresenceCache cache = new(npcTypeCount: 4);
    NpcActivePresenceScanSystem.Rebuild(
      cache,
      scanRevision: 1,
      entries: [new NpcActivePresenceScanEntry(3, true)]);

    RequireThrows<ArgumentOutOfRangeException>(
      () => NpcActivePresenceScanSystem.Rebuild(
        cache,
        scanRevision: 1,
        entries: Array.Empty<NpcActivePresenceScanEntry>()),
      "a scan revision must increase");

    cache.Invalidate();

    Require(!cache.IsValid, "invalidated cache must not remain valid");
    Require(!cache.TryGetActive(1, 3, out _), "invalidated point read must fail");
    Require(!cache.TryGetActiveNpcTypes(1, out int[] activeTypes),
      "invalidated snapshot read must fail");
    Require(activeTypes.Length == 0, "invalidated snapshot must be empty");
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
