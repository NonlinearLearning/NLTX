using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Moves pre-TileEntity mannequin and weapons-rack state into session owners.</summary>
internal static class LegacyWorldTileEntityMigration
{
  private const int LegacyWorldEdgeClearance = 5;
  private const ushort ActiveTileFlag = 0x20;
  private const ushort LegacyMaleMannequinTile = 128;
  private const ushort LegacyFemaleMannequinTile = 269;
  private const ushort LegacyWeaponsRackTile = 334;
  private const ushort DisplayDollTile = 470;
  private const ushort WeaponsRackTile = 471;
  private const byte DisplayDollType = 3;
  private const byte WeaponsRackType = 4;
  private const int DisplayDollItemSlotCount = 19;

  public static WorldStorageOperationResult Apply(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!session.IsComplete || !session.IsPublicationUncertain || session.IsPublished ||
        !session.Lifecycle.IsGeneratingOrLoadingWorld)
    {
      return Invalid("Legacy TileEntity migration requires a gated candidate session.");
    }

    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      TileMapStore tileMap = session.Storage.TileMap;
      int width = tileMap.Width;
      int height = tileMap.Height;
      var mannequins = new List<TileCoordinate>();
      var weaponsRacks = new List<TileCoordinate>();

      for (int x = 0; x < width; x++)
      {
        cancellationToken.ThrowIfCancellationRequested();
        for (int y = 0; y < height; y++)
        {
          TileCellState tile = tileMap.GetTile(x, y);
          if ((tile.Type == LegacyMaleMannequinTile ||
              tile.Type == LegacyFemaleMannequinTile) &&
              tile.FrameY == 0 && (tile.FrameX % 100 == 0 || tile.FrameX % 100 == 36))
          {
            mannequins.Add(new TileCoordinate(x, y));
          }

          if (tile.Type == LegacyWeaponsRackTile && tile.FrameY == 0 && tile.FrameX % 54 == 0)
          {
            weaponsRacks.Add(new TileCoordinate(x, y));
          }
        }
      }

      if (mannequins.Count == 0 && weaponsRacks.Count == 0)
      {
        return WorldStorageOperationResult.Success;
      }

      IReadOnlyList<TileEntitySnapshot> persistedEntities =
        session.Storage.TileEntities.CreateSnapshot();
      var entities = new List<TileEntitySnapshot>(persistedEntities);
      var anchors = new HashSet<TileCoordinate>();
      foreach (TileEntitySnapshot entity in persistedEntities)
      {
        if (!anchors.Add(entity.Anchor))
        {
          return Invalid("The loaded TileEntity store contains duplicate anchors.");
        }
      }

      var tileChanges = new Dictionary<TileCoordinate, TileCellState>();
      int nextId = session.Storage.TileEntities.NextId;
      foreach (TileCoordinate anchor in mannequins)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsInLegacyWorldBounds(anchor, width, height))
        {
          continue;
        }

        if (!IsActive(GetTile(tileMap, tileChanges, anchor)))
        {
          continue;
        }

        if (!anchors.Add(anchor))
        {
          return Invalid("A legacy mannequin overlaps a loaded TileEntity anchor.");
        }

        TileCellState topTile = GetTile(tileMap, tileChanges, anchor);
        TileCellState middleTile = GetTile(
          tileMap,
          tileChanges,
          new TileCoordinate(anchor.X, anchor.Y + 1));
        TileCellState bottomTile = GetTile(
          tileMap,
          tileChanges,
          new TileCoordinate(anchor.X, anchor.Y + 2));
        ItemState[] items = CreateDisplayDollItems(
          topTile.FrameX,
          middleTile.FrameX,
          bottomTile.FrameX);

        entities.Add(new TileEntitySnapshot(
          new TileEntityId(nextId),
          new TileEntityTypeId(DisplayDollType),
          anchor,
          items));
        nextId = checked(nextId + 1);

        for (int xOffset = 0; xOffset < 2; xOffset++)
        {
          for (int yOffset = 0; yOffset < 3; yOffset++)
          {
            var coordinate = new TileCoordinate(anchor.X + xOffset, anchor.Y + yOffset);
            TileCellState tile = GetTile(tileMap, tileChanges, coordinate);
            short normalizedFrameX = checked((short)(tile.FrameX % 100));
            if (tile.Type == LegacyFemaleMannequinTile)
            {
              normalizedFrameX = checked((short)(normalizedFrameX + 72));
            }

            tile.Type = DisplayDollTile;
            tile.FrameX = normalizedFrameX;
            tileChanges[coordinate] = tile;
          }
        }
      }

      foreach (TileCoordinate anchor in weaponsRacks)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsInLegacyWorldBounds(anchor, width, height))
        {
          continue;
        }

        if (!IsActive(GetTile(tileMap, tileChanges, anchor)))
        {
          continue;
        }

        if (!anchors.Add(anchor))
        {
          return Invalid("A legacy weapons rack overlaps a loaded TileEntity anchor.");
        }

        TileCellState topTile = GetTile(tileMap, tileChanges, anchor);
        TileCellState lowerLeft = GetTile(
          tileMap,
          tileChanges,
          new TileCoordinate(anchor.X, anchor.Y + 1));
        TileCellState lowerRight = GetTile(
          tileMap,
          tileChanges,
          new TileCoordinate(anchor.X + 1, anchor.Y + 1));
        ItemState item = CreateWeaponsRackItem(lowerLeft.FrameX, lowerRight.FrameX);
        bool facingRight = topTile.FrameX >= 54;

        entities.Add(new TileEntitySnapshot(
          new TileEntityId(nextId),
          new TileEntityTypeId(WeaponsRackType),
          anchor,
          new[] { item }));
        nextId = checked(nextId + 1);

        for (int xOffset = 0; xOffset < 3; xOffset++)
        {
          for (int yOffset = 0; yOffset < 3; yOffset++)
          {
            var coordinate = new TileCoordinate(anchor.X + xOffset, anchor.Y + yOffset);
            TileCellState tile = GetTile(tileMap, tileChanges, coordinate);
            tile.Type = WeaponsRackTile;
            tile.FrameX = checked((short)((facingRight ? 54 : 0) + xOffset * 18));
            tile.FrameY = checked((short)(yOffset * 18));
            tileChanges[coordinate] = tile;
          }
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      var replacement = new TileEntityStoreSnapshot(entities, nextId);
      foreach ((TileCoordinate coordinate, TileCellState tile) in tileChanges)
      {
        tileMap.CommitTile(coordinate.X, coordinate.Y, tile);
      }

      session.Storage.TileEntities.CommitRuntimeSnapshot(replacement);
      session.Storage.TileEntityUpdates.Replace(
        session.Storage.TileEntities.CreateScheduledIdSnapshot());
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
    catch (OverflowException exception)
    {
      return Invalid(exception.Message);
    }
  }

  private static ItemState[] CreateDisplayDollItems(
    short headFrame,
    short bodyFrame,
    short legFrame)
  {
    var items = new ItemState[DisplayDollItemSlotCount];
    items[0] = ResolveMannequinItem(headFrame, Item.headType, "head");
    items[1] = ResolveMannequinItem(bodyFrame, Item.bodyType, "body");
    items[2] = ResolveMannequinItem(legFrame, Item.legType, "leg");
    return items;
  }

  private static ItemState ResolveMannequinItem(
    short frame,
    IReadOnlyList<int> itemTypesByArmorSlot,
    string slotName)
  {
    if (frame < 0)
    {
      throw new InvalidDataException(
        $"A legacy mannequin has a negative {slotName} equipment frame.");
    }

    int armorSlot = frame / 100;
    if (armorSlot == 0)
    {
      return new ItemState(0, 0, 0);
    }

    if (armorSlot >= itemTypesByArmorSlot.Count)
    {
      throw new InvalidDataException(
        $"The legacy mannequin {slotName} slot {armorSlot} is outside the runtime catalog.");
    }

    int itemType = itemTypesByArmorSlot[armorSlot];
    if (itemType <= 0 || itemType >= ItemID.Count)
    {
      throw new InvalidDataException(
        $"The legacy mannequin {slotName} slot {armorSlot} has no resolved item type.");
    }

    return new ItemState(itemType, 0, 1);
  }

  private static ItemState CreateWeaponsRackItem(short itemFrame, short prefixFrame)
  {
    if (itemFrame < 5000)
    {
      return new ItemState(0, 0, 0);
    }

    int itemType = itemFrame % 5000 - 100;
    int prefix = prefixFrame - (prefixFrame >= 25000 ? 25000 : 10000);
    if (itemType <= 0 || itemType >= ItemID.Count)
    {
      throw new InvalidDataException(
        $"A legacy weapons rack item type {itemType} cannot be restored.");
    }

    if (prefix < 0 || prefix >= PrefixID.Count)
    {
      throw new InvalidDataException(
        $"A legacy weapons rack prefix {prefix} cannot be restored.");
    }

    return new ItemState(itemType, checked((byte)prefix), 1);
  }

  private static TileCellState GetTile(
    TileMapStore tileMap,
    IReadOnlyDictionary<TileCoordinate, TileCellState> tileChanges,
    TileCoordinate coordinate)
  {
    return tileChanges.TryGetValue(coordinate, out TileCellState changed)
      ? changed
      : tileMap.GetTile(coordinate.X, coordinate.Y);
  }

  private static bool IsInLegacyWorldBounds(TileCoordinate anchor, int width, int height)
  {
    return anchor.X >= LegacyWorldEdgeClearance && anchor.X < width - LegacyWorldEdgeClearance &&
      anchor.Y >= LegacyWorldEdgeClearance && anchor.Y < height - LegacyWorldEdgeClearance;
  }

  private static bool IsActive(TileCellState tile)
  {
    return (tile.TileHeader & ActiveTileFlag) != 0;
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
