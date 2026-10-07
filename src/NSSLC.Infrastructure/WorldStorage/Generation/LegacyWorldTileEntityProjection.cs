using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;
using RuntimeTileEntities = NSSLC.WorldGeneration.GameContent.Tile_Entities;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Materializes committed TileEntity snapshots into the generated-world runtime.</summary>
public static class LegacyWorldTileEntityProjection
{
  public static WorldStorageOperationResult Publish(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(session);
    return ProjectAll(session, cancellationToken, allowPublished: false);
  }

  internal static WorldStorageOperationResult ReprojectPublished(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    return ProjectAll(session, CancellationToken.None, allowPublished: true);
  }

  private static WorldStorageOperationResult ProjectAll(
    LoadedWorldSession session,
    CancellationToken cancellationToken,
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
      return Invalid("TileEntity state can only be published during a gated session commit.");
    }

    try
    {
      if (Main.tile is null || Main.maxTilesX != session.Storage.TileMap.Width ||
          Main.maxTilesY != session.Storage.TileMap.Height ||
          Main.tile.GetLength(0) != session.Storage.TileMap.Width ||
          Main.tile.GetLength(1) != session.Storage.TileMap.Height)
      {
        return Invalid("The runtime tile map must be published before TileEntity state.");
      }

      IReadOnlyList<TileEntitySnapshot> snapshots = session.Storage.TileEntities.CreateSnapshot();
      int nextId = session.Storage.TileEntities.NextId;
      var staged = new List<TileEntity>(snapshots.Count);
      var ids = new HashSet<int>();
      var anchors = new HashSet<(int X, int Y)>();
      foreach (TileEntitySnapshot snapshot in snapshots)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (snapshot.Id.Value < 0 || snapshot.Id.Value >= nextId ||
            !ids.Add(snapshot.Id.Value))
        {
          return Invalid("Loaded TileEntity IDs are invalid or duplicated.");
        }
        if (snapshot.Anchor.X < 0 || snapshot.Anchor.X >= Main.maxTilesX ||
            snapshot.Anchor.Y < 0 || snapshot.Anchor.Y >= Main.maxTilesY ||
            !anchors.Add((snapshot.Anchor.X, snapshot.Anchor.Y)))
        {
          return Invalid("Loaded TileEntity anchors are outside the runtime tile map or duplicated.");
        }

        Tile tile = Main.tile[snapshot.Anchor.X, snapshot.Anchor.Y];
        if (tile is null)
        {
          return Invalid("A loaded TileEntity anchor has no runtime tile.");
        }
        staged.Add(CreateRuntimeEntity(snapshot, tile.type));
      }

      cancellationToken.ThrowIfCancellationRequested();
      TileEntity.ReplaceLoaded(staged, nextId);
      foreach (TileEntity entity in staged)
      {
        cancellationToken.ThrowIfCancellationRequested();
        entity.OnWorldLoaded();
      }
      return WorldStorageOperationResult.Success;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (InvalidDataException exception)
    {
      return Invalid(exception.Message);
    }
    catch (ArgumentException exception)
    {
      return Invalid(exception.Message);
    }
    catch (InvalidOperationException exception)
    {
      return Invalid(exception.Message);
    }
  }

  public static WorldStorageOperationResult PublishCommittedSnapshot(
      LoadedWorldSession session,
      TileEntitySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!session.IsComplete || !session.IsPublished || session.IsPublicationUncertain ||
        session.Lifecycle.IsGeneratingOrLoadingWorld)
    {
      return Invalid("TileEntity updates can only be projected from a published session.");
    }

    if (snapshot.Anchor.X < 0 || snapshot.Anchor.X >= Main.maxTilesX ||
        snapshot.Anchor.Y < 0 || snapshot.Anchor.Y >= Main.maxTilesY ||
        Main.tile is null || Main.tile[snapshot.Anchor.X, snapshot.Anchor.Y] is not Tile tile)
    {
      return Invalid("The runtime TileEntity anchor is outside the published tile map.");
    }

    TileEntity replacement;
    try
    {
      replacement = CreateRuntimeEntity(snapshot, tile.type);
    }
    catch (InvalidDataException exception)
    {
      return Invalid(exception.Message);
    }
    catch (ArgumentException exception)
    {
      return Invalid(exception.Message);
    }

    return TileEntity.ReplaceAtAnchor(replacement)
      ? WorldStorageOperationResult.Success
      : Invalid("The runtime TileEntity identity no longer matches the committed state.");
  }

  public static WorldStorageOperationResult RemoveCommittedSnapshot(
      LoadedWorldSession session,
      TileEntitySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!session.IsComplete || !session.IsPublished || session.IsPublicationUncertain ||
        session.Lifecycle.IsGeneratingOrLoadingWorld)
    {
      return Invalid("TileEntity updates can only be projected from a published session.");
    }

    return TileEntity.RemoveAtAnchor(
      snapshot.Anchor.X,
      snapshot.Anchor.Y,
      snapshot.Id.Value)
        ? WorldStorageOperationResult.Success
        : Invalid("The runtime TileEntity identity no longer matches the removed state.");
  }

  private static TileEntity CreateRuntimeEntity(TileEntitySnapshot snapshot, int tileType)
  {
    int id = snapshot.Id.Value;
    int x = snapshot.Anchor.X;
    int y = snapshot.Anchor.Y;
    switch (snapshot.Type.Value)
    {
      case 0:
        RequireItemCount(snapshot, 0);
        var trainingDummy = TileEntity.CreateForRestore<RuntimeTileEntities.TETrainingDummy>(
            id, x, y, tileType);
        trainingDummy.npc = snapshot.NpcIndex;
        return trainingDummy;
      case 1:
        RequireItemCount(snapshot, 1);
        var itemFrame = TileEntity.CreateForRestore<RuntimeTileEntities.TEItemFrame>(
            id, x, y, tileType);
        itemFrame.item = RestoreItem(snapshot.Items[0]);
        return itemFrame;
      case 2:
        RequireItemCount(snapshot, 0);
        var logicSensor = TileEntity.CreateForRestore<NSSLC.WorldGeneration.TELogicSensor>(
            id, x, y, tileType);
        logicSensor.logicCheck = snapshot.LogicCheck;
        logicSensor.On = snapshot.LogicOn;
        return logicSensor;
      case 3:
        RequireItemCount(snapshot, 19);
        var displayDoll = TileEntity.CreateForRestore<NSSLC.WorldGeneration.TEDisplayDoll>(
            id, x, y, tileType);
        for (int index = 0; index < snapshot.Items.Count; index++)
        {
          displayDoll.items[index] = RestoreItem(snapshot.Items[index]);
        }
        displayDoll.pose = snapshot.Pose;
        return displayDoll;
      case 4:
        RequireItemCount(snapshot, 1);
        var weaponsRack = TileEntity.CreateForRestore<RuntimeTileEntities.TEWeaponsRack>(
            id, x, y, tileType);
        weaponsRack.item = RestoreItem(snapshot.Items[0]);
        return weaponsRack;
      case 5:
        RequireItemCount(snapshot, 4);
        var hatRack = TileEntity.CreateForRestore<NSSLC.WorldGeneration.TEHatRack>(
            id, x, y, tileType);
        for (int index = 0; index < snapshot.Items.Count; index++)
        {
          hatRack.items[index] = RestoreItem(snapshot.Items[index]);
        }
        return hatRack;
      case 6:
        RequireItemCount(snapshot, 1);
        var foodPlatter = TileEntity.CreateForRestore<RuntimeTileEntities.TEFoodPlatter>(
            id, x, y, tileType);
        foodPlatter.item = RestoreItem(snapshot.Items[0]);
        return foodPlatter;
      case 7:
        RequireItemCount(snapshot, 0);
        return TileEntity.CreateForRestore<NSSLC.WorldGeneration.TETeleportationPylon>(
            id, x, y, tileType);
      case 8:
        RequireItemCount(snapshot, 0);
        return TileEntity.CreateForRestore<RuntimeTileEntities.TEDeadCellsDisplayJar>(
            id, x, y, tileType);
      case 9:
        RequireItemCount(snapshot, 0);
        return TileEntity.CreateForRestore<NSSLC.WorldGeneration.TEKiteAnchor>(
            id, x, y, tileType);
      case 10:
        RequireItemCount(snapshot, 0);
        return TileEntity.CreateForRestore<NSSLC.WorldGeneration.TECritterAnchor>(
            id, x, y, tileType);
      default:
        throw new InvalidDataException(
          $"Unsupported loaded TileEntity type {snapshot.Type.Value}.");
    }
  }

  private static Item RestoreItem(ItemState state)
  {
    var item = new Item();
    if (state.Type < 0)
    {
      item.netDefaults(state.Type);
    }
    else if (state.Type > 0 && state.Type < ItemID.Count)
    {
      item.SetDefaults(state.Type);
    }
    else
    {
      item.type = state.Type;
    }

    if (state.Prefix < PrefixID.Count)
    {
      item.Prefix(state.Prefix);
    }
    else
    {
      item.prefix = state.Prefix;
    }

    item.stack = state.Stack;
    return item;
  }

  private static void RequireItemCount(TileEntitySnapshot snapshot, int expected)
  {
    if (snapshot.Items.Count != expected)
    {
      throw new InvalidDataException(
        $"TileEntity type {snapshot.Type.Value} has an invalid item-slot count.");
    }
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
