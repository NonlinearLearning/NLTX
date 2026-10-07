using System;
using System.Collections.Generic;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Projects committed chest and sign snapshots into the legacy runtime tables.</summary>
internal static class LegacyWorldChestAndSignProjection
{
  public static WorldStorageOperationResult Publish(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return Project(session, allowPublished: false);
  }

  internal static WorldStorageOperationResult ReprojectPublished(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return Project(session, allowPublished: true);
  }

  private static WorldStorageOperationResult Project(
    LoadedWorldSession session,
    bool allowPublished)
  {
    bool eligibleSession = allowPublished
      ? session.IsComplete && session.IsPublished && !session.IsPublicationUncertain &&
        !session.Lifecycle.IsGeneratingOrLoadingWorld &&
        ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session)
      : session.IsComplete && session.IsPublicationUncertain && !session.IsPublished &&
        session.Lifecycle.IsGeneratingOrLoadingWorld;
    if (!eligibleSession)
    {
      return Invalid("Chest and sign data can only be published during a gated session commit.");
    }

    IReadOnlyList<WorldChestSnapshot> chestSnapshots =
      session.Storage.WorldContainers.CreateSnapshot();
    IReadOnlyList<WorldSignSnapshot> signSnapshots =
      session.Storage.WorldSigns.CreateSnapshot();
    if (chestSnapshots.Count > Main.chest.Length || signSnapshots.Count > Main.sign.Length)
    {
      return Invalid(
        $"Loaded chest/sign counts ({chestSnapshots.Count}/{signSnapshots.Count}) exceed " +
        $"legacy runtime capacities ({Main.chest.Length}/{Main.sign.Length}).");
    }

    var stagedChests = new Chest[chestSnapshots.Count];
    var chestAnchors = new HashSet<(int X, int Y)>();
    for (int index = 0; index < chestSnapshots.Count; index++)
    {
      WorldChestSnapshot snapshot = chestSnapshots[index];
      if (!IsValidChestAnchor(snapshot.Anchor) ||
          !chestAnchors.Add((snapshot.Anchor.X, snapshot.Anchor.Y)))
      {
        return Invalid("Loaded chest anchors are invalid or duplicated.");
      }

      Chest chest = Chest.CreateOutOfArray(
        index,
        snapshot.Anchor.X,
        snapshot.Anchor.Y,
        maxItems: 40);
      if (snapshot.Items.Count > chest.item.Length)
      {
        return Invalid("Loaded chest data exceeds the legacy item-slot capacity.");
      }

      chest.name = snapshot.Name;
      for (int itemIndex = 0; itemIndex < snapshot.Items.Count; itemIndex++)
      {
        ItemState itemState = snapshot.Items[itemIndex];
        if (itemState.Stack < short.MinValue || itemState.Stack > short.MaxValue ||
            (itemState.Stack > 0 && itemState.Type == 0) ||
            (itemState.Stack != 0 && itemState.Type >= ItemID.Count))
        {
          return Invalid("Loaded chest data contains an invalid item.");
        }

        if (itemState.IsEmpty)
        {
          continue;
        }

        Item item = chest.item[itemIndex];
        item.netDefaults(itemState.Type);
        if (itemState.Stack > 0)
        {
          item.stack = itemState.Stack;
        }
        if (itemState.Prefix < PrefixID.Count)
        {
          item.Prefix(itemState.Prefix);
        }
        else
        {
          item.prefix = itemState.Prefix;
        }
        if (itemState.Stack < 0)
        {
          item.stack = 1;
        }
      }

      stagedChests[index] = chest;
    }

    var stagedSigns = new Sign[Main.sign.Length];
    var signAnchors = new HashSet<(int X, int Y)>();
    for (int index = 0; index < signSnapshots.Count; index++)
    {
      WorldSignSnapshot snapshot = signSnapshots[index];
      if (!IsValidSignAnchor(snapshot.Anchor) ||
          !signAnchors.Add((snapshot.Anchor.X, snapshot.Anchor.Y)))
      {
        return Invalid("Loaded sign anchors are invalid or duplicated.");
      }

      stagedSigns[index] = new Sign
      {
        x = snapshot.Anchor.X,
        y = snapshot.Anchor.Y,
        text = snapshot.Text
      };
    }

    Chest.Clear();
    for (int index = 0; index < stagedChests.Length; index++)
    {
      Chest.Assign(stagedChests[index]);
    }

    Array.Clear(Main.sign, 0, Main.sign.Length);
    Array.Copy(stagedSigns, Main.sign, stagedSigns.Length);
    return WorldStorageOperationResult.Success;
  }

  private static bool IsValidChestAnchor(TileCoordinate anchor)
  {
    return Main.maxTilesX > 1 && Main.maxTilesY > 1 &&
      anchor.X >= 0 && anchor.X < Main.maxTilesX - 1 &&
      anchor.Y >= 0 && anchor.Y < Main.maxTilesY - 1;
  }

  private static bool IsValidSignAnchor(TileCoordinate anchor)
  {
    return anchor.X >= 0 && anchor.X < Main.maxTilesX &&
      anchor.Y >= 0 && anchor.Y < Main.maxTilesY;
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
