using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private const int StandardTileReachTiles = 5;
  private const int TreeHarvestReachTiles = StandardTileReachTiles + 1;

  private string? HandleTileManipulation(InboundPacketEnvelope envelope,
      TileManipulationPacket packet, List<OutboundPacketIntent> intents) {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope, 17, [SessionStage.Active], binding => {
        WorldHeaderCapture header = _composition.WorldOwner.CaptureWorldHeader();
        var coordinate = new TileCoordinate(packet.X, packet.Y);
        WorldGeometry geometry = header.Header.Geometry;
        if (coordinate.X < 3 || coordinate.Y < 3 || coordinate.X >= geometry.Width - 3 ||
            coordinate.Y >= geometry.Height - 3) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The tile request is outside the world margin.");
        }
        var section = new WorldSectionCoordinate(coordinate.X / WorldGeometry.SectionWidth,
          coordinate.Y / WorldGeometry.SectionHeight);
        if (_worldSectionRequests?.HasQueuedSection(envelope.Epoch, header.WorldEpoch,
            section) != true) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "The tile request refers to a section not delivered to this session.");
        }
        WorldSpatialSnapshot before = _composition.WorldOwner.CaptureSpatialSnapshot(
          new WorldSectionBounds(coordinate.X, coordinate.Y, 1, 1));
        WorldTile tile = before.GetTile(coordinate);
        if (packet.Action == 1) {
          return HandleBasicTilePlacement(envelope, packet, binding, header, coordinate,
            tile, intents);
        }
        if (packet.TileOrWallType == 1 && packet.Action is 0 or 2 or 4) {
          intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(packet,
            _composition.SessionAdmission.CaptureActiveSessionEpochs(), envelope.Epoch,
            header.Version));
          return OwnerCommitResult.Applied(header.Version);
        }
        if (packet.Action is not (0 or 4)) {
          AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
            "This tile action requires its placement, wall or object rule owner.");
        }
        if (!tile.Active) {
          AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
          return OwnerCommitResult.Applied(header.Version);
        }
        if (tile.Type == 5 && packet.Action is 0 or 4) {
          return HandleOrdinaryTreeHarvest(envelope, packet, binding, header, coordinate,
            tile, intents);
        }
        bool singleCellPlant = tile.Type is 3 or 73 or 110;
        if ((tile.Type >= header.Facts.FrameImportant.Count ||
             header.Facts.FrameImportant[tile.Type]) && !singleCellPlant ||
            !WorldTileDropRules.TryGetDrop(tile, out int itemType)) {
          AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            $"Tile type {tile.Type} frame {tile.FrameX} has no supported single-cell drop rule.");
        }
        if (packet.Action == 4) {
          itemType = 0;
        }
        var patch = new WorldTilePatch(coordinate, Active: false, Type: 0,
          FrameX: 0, FrameY: 0, HalfBlock: false, Slope: 0,
          Actuator: false, Inactive: false);
        if (!_composition.WorldOwner.TryPrepareTilePatches(header.WorldEpoch, header.Version,
            [patch], true, out WorldTileMutationPreparation? tiles,
            out WorldValidationResult validation) || tiles is null) {
          AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
          return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
            string.Join("; ", validation.Errors));
        }
        var spawns = new List<WorldItemSpawn>();
        if (itemType > 0) {
          IReadOnlyList<int> slots = _composition.WorldItemStore.CaptureFreeSlots(1);
          if (slots.Count == 0) {
            AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
            return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
              "All world-item slots are occupied; terrain and its drop are retained.");
          }
          spawns.Add(new WorldItemSpawn(binding.PlayerHandle.WorldScope, slots[0],
            new WorldItemMutation(new InventoryItemState(itemType, 1, 0, false),
              new Vector2(coordinate.X * 16, coordinate.Y * 16), new Vector2(0, -1),
              0, false, 0, Runtime: new WorldItemRuntime())));
        }
        // Pre-encode the normalized result before either owner publishes its changes.
        var correction = CreateTileCorrection(coordinate, tiles.Tiles[0].Value);
        IReadOnlyList<SessionEpoch> recipients =
          _composition.SessionAdmission.CaptureActiveSessionEpochs();
        OutboundPacketIntent committedTile = _composition.Version4WireProfile.CreateOutboundIntent(
          correction, recipients, null, tiles.Result.Version!.Value);
        _composition.WorldItemStore.Projections.SetTargets(binding.PlayerHandle.WorldScope,
          recipients);
        OwnerCommitResult prepared = _composition.WorldItemTransactions.Prepare(
          [], spawns, [], null, [new TileItemTransactionParticipant(tiles)],
          out WorldItemTransactionCoordinator.PreparedTransaction? transaction);
        OwnerCommitResult committed = transaction is null ? prepared : transaction.Commit().Result;
        if (committed.Status != OwnerCommitStatus.Applied) {
          AddTileCorrection(envelope.Epoch, coordinate, tile, header.Version, intents);
          return committed;
        }
        intents.Add(committedTile);
        ServerLog.Info("server.tile.destroyed",
          $"epoch={envelope.Epoch.Value} x={packet.X} y={packet.Y} " +
          $"tile={tile.Type} drop={itemType} version={tiles.Result.Version}");
        return committed;
      });
    return result.Status == OwnerCommitStatus.Applied ? null : result.Detail;
  }

  private OwnerCommitResult HandleBasicTilePlacement(InboundPacketEnvelope envelope,
      TileManipulationPacket packet, SessionBinding binding, WorldHeaderCapture header,
      TileCoordinate coordinate, WorldTile currentTile, List<OutboundPacketIntent> intents) {
    if (!_composition.Players.TryGet(binding.PlayerHandle, out var player) || player is null) {
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        "The authenticated player's state is unavailable for tile placement.");
    }
    if (!_composition.InventoryStore.TryCapture(binding.PlayerHandle.Id,
        out InventorySnapshot? inventory) || inventory is null) {
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        "The authenticated player's inventory is unavailable for tile placement.");
    }

    int selectedSlotIndex = player.Movement.SelectedItem;
    InventorySlotSnapshot? selectedSlot = selectedSlotIndex < 10
      ? inventory.Slots[selectedSlotIndex]
      : null;
    OwnerCommitResult Reject(string detail) {
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      if (selectedSlot is not null) {
        AddInventoryCorrection(envelope.Epoch, binding.PlayerSlot, selectedSlot,
          inventory.Revision, intents);
      }
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, detail);
    }

    if (player.IsDead || !IsWithinStandardTileReach(player, coordinate)) {
      return Reject("The tile placement is outside the authenticated player's reach.");
    }
    if (selectedSlot is null || selectedSlot.Item.IsEmpty) {
      return Reject("The selected hotbar slot has no placeable block.");
    }
    if (packet.Style != 0 || !TryGetBasicPlacementTile(selectedSlot.Item.Type,
        packet.TileOrWallType, out ushort tileType)) {
      return Reject("The selected item does not match a supported basic tile placement.");
    }
    if (currentTile.Active) {
      return Reject("The target tile is occupied.");
    }

    var patch = new WorldTilePatch(coordinate, Active: true, Type: tileType,
      FrameX: -1, FrameY: -1, TileColor: 0, TileHeader3: 0,
      HalfBlock: false, Slope: 0, Actuator: false, Inactive: false);
    if (!_composition.WorldOwner.TryPrepareTilePatches(header.WorldEpoch, header.Version,
        [patch], true, out WorldTileMutationPreparation? tiles,
        out WorldValidationResult validation) || tiles is null) {
      return Reject(string.Join("; ", validation.Errors));
    }

    OwnerCommitResult inventoryPrepared = InventoryItemPlacementPreparation.TryPrepare(
      _composition.InventoryStore, inventory, selectedSlot, out var inventoryDebit);
    if (inventoryPrepared.Status != OwnerCommitStatus.Applied || inventoryDebit is null) {
      return Reject(inventoryPrepared.Detail ?? "The selected block could not be debited.");
    }

    var correction = CreateTileCorrection(coordinate, tiles.Tiles[0].Value);
    OutboundPacketIntent committedTile = _composition.Version4WireProfile.CreateOutboundIntent(
      correction, _composition.SessionAdmission.CaptureActiveSessionEpochs(), null,
      tiles.Result.Version!.Value);
    InventorySlotSnapshot updatedSlot = inventoryDebit.Snapshot.Slots[selectedSlotIndex];
    OutboundPacketIntent committedInventory = _composition.Version4WireProfile.CreateOutboundIntent(
      CreateInventoryCorrection(binding.PlayerSlot, updatedSlot), [envelope.Epoch], null,
      inventoryDebit.Snapshot.Revision);

    OwnerCommitResult prepared = _composition.WorldItemTransactions.Prepare(
      [], [], [], null, [new TileItemTransactionParticipant(tiles), inventoryDebit],
      out WorldItemTransactionCoordinator.PreparedTransaction? transaction);
    OwnerCommitResult committed = transaction is null ? prepared : transaction.Commit().Result;
    if (committed.Status != OwnerCommitStatus.Applied) {
      return Reject(committed.Detail ?? "The tile and inventory transaction was rejected.");
    }

    intents.Add(committedTile);
    intents.Add(committedInventory);
    ServerLog.Info("server.tile.placed",
      $"epoch={envelope.Epoch.Value} x={packet.X} y={packet.Y} " +
      $"item={selectedSlot.Item.Type} tile={tileType} version={tiles.Result.Version}");
    return committed;
  }

  private OwnerCommitResult HandleOrdinaryTreeHarvest(InboundPacketEnvelope envelope,
      TileManipulationPacket packet, SessionBinding binding, WorldHeaderCapture header,
      TileCoordinate coordinate, WorldTile currentTile, List<OutboundPacketIntent> intents) {
    if (!_composition.Players.TryGet(binding.PlayerHandle, out var player) || player is null) {
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The tree request is outside the authenticated player's reach.");
    }
    if (player.IsDead || !IsWithinTreeHarvestReach(player, coordinate)) {
      var position = player.Movement.Position;
      ServerLog.Warning("server.tree.harvest.rejected",
        $"epoch={envelope.Epoch.Value} x={packet.X} y={packet.Y} action={packet.Action} " +
        $"frame=({currentTile.FrameX},{currentTile.FrameY}) " +
        $"player=({position.X:F1},{position.Y:F1}) " +
        "reason=outside-reach-or-dead");
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The tree request is outside the authenticated player's reach.");
    }

    WorldGeometry geometry = header.Header.Geometry;
    int left = Math.Max(3, coordinate.X - 8);
    int top = Math.Max(3, coordinate.Y - 64);
    int right = Math.Min(geometry.Width - 4, coordinate.X + 8);
    int bottom = Math.Min(geometry.Height - 4, coordinate.Y + 64);
    var treeBounds = new WorldSectionBounds(left, top, right - left + 1,
      bottom - top + 1);
    WorldSpatialSnapshot treeSnapshot = _composition.WorldOwner.CaptureSpatialSnapshot(treeBounds);
    if (!WorldTreeHarvestRules.TryCollectOrdinaryTree(treeSnapshot, coordinate,
        out IReadOnlyList<TileCoordinate> treeTiles, out string rejection)) {
      ServerLog.Warning("server.tree.harvest.rejected",
        $"epoch={envelope.Epoch.Value} x={packet.X} y={packet.Y} action={packet.Action} " +
        $"frame=({currentTile.FrameX},{currentTile.FrameY}) reason=\"{rejection}\"");
      AddTileCorrection(envelope.Epoch, coordinate, currentTile, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, rejection);
    }

    foreach (TileCoordinate treeTile in treeTiles) {
      var section = new WorldSectionCoordinate(treeTile.X / WorldGeometry.SectionWidth,
        treeTile.Y / WorldGeometry.SectionHeight);
      if (_worldSectionRequests?.HasQueuedSection(envelope.Epoch, header.WorldEpoch,
          section) != true) {
        AddTileCorrections(envelope.Epoch, treeSnapshot, treeTiles, header.Version, intents);
        return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "The tree includes a section not delivered to this session.");
      }
    }

    int itemType = packet.Action == 0 ? WorldTreeHarvestRules.WoodItemType : 0;
    int axePower = itemType > 0 ? CaptureSelectedTreeAxePower(player) : 0;
    int bonusWoodDropCount = itemType > 0
      ? CountTreeWoodBonuses(treeTiles.Count, axePower, _composition.TreeHarvestRandom)
      : 0;
    int itemStack = itemType > 0
      ? WorldTreeHarvestRules.GetWoodDropStack(treeTiles.Count, bonusWoodDropCount)
      : 0;
    var patches = treeTiles.Select(treeTile => new WorldTilePatch(treeTile,
      Active: false, Type: 0, FrameX: 0, FrameY: 0, TileColor: 0,
      TileHeader3: 0, HalfBlock: false, Slope: 0, Actuator: false,
      Inactive: false)).ToArray();
    if (!_composition.WorldOwner.TryPrepareTilePatches(header.WorldEpoch, header.Version,
        patches, true, out WorldTileMutationPreparation? tiles,
        out WorldValidationResult validation) || tiles is null) {
      AddTileCorrections(envelope.Epoch, treeSnapshot, treeTiles, header.Version, intents);
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        string.Join("; ", validation.Errors));
    }

    var spawns = new List<WorldItemSpawn>();
    if (itemType > 0) {
      IReadOnlyList<int> slots = _composition.WorldItemStore.CaptureFreeSlots(1);
      if (slots.Count == 0) {
        AddTileCorrections(envelope.Epoch, treeSnapshot, treeTiles, header.Version, intents);
        return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
          "All world-item slots are occupied; the tree and its wood are retained.");
      }
      spawns.Add(new WorldItemSpawn(binding.PlayerHandle.WorldScope, slots[0],
        new WorldItemMutation(new InventoryItemState(itemType, itemStack, 0, false),
          new Vector2(coordinate.X * 16, coordinate.Y * 16), new Vector2(0, -1),
          0, false, 0, Runtime: new WorldItemRuntime())));
    }

    IReadOnlyList<SessionEpoch> recipients =
      _composition.SessionAdmission.CaptureActiveSessionEpochs();
    OutboundPacketIntent[] committedTiles = tiles.Tiles.Select(change =>
      _composition.Version4WireProfile.CreateOutboundIntent(
        CreateTileCorrection(change.Key, change.Value), recipients, null,
        tiles.Result.Version!.Value)).ToArray();
    _composition.WorldItemStore.Projections.SetTargets(binding.PlayerHandle.WorldScope,
      recipients);
    OwnerCommitResult prepared = _composition.WorldItemTransactions.Prepare(
      [], spawns, [], null, [new TileItemTransactionParticipant(tiles)],
      out WorldItemTransactionCoordinator.PreparedTransaction? transaction);
    OwnerCommitResult committed = transaction is null ? prepared : transaction.Commit().Result;
    if (committed.Status != OwnerCommitStatus.Applied) {
      AddTileCorrections(envelope.Epoch, treeSnapshot, treeTiles, header.Version, intents);
      return committed;
    }

    intents.AddRange(committedTiles);
    ServerLog.Info("server.tree.harvested",
      $"epoch={envelope.Epoch.Value} x={packet.X} y={packet.Y} " +
      $"tiles={treeTiles.Count} item={itemType} stack={itemStack} " +
      $"axePower={axePower} bonusWood={bonusWoodDropCount} " +
      $"version={tiles.Result.Version}");
    return committed;
  }

  private int CaptureSelectedTreeAxePower(PlayerEntity player) {
    if (!_composition.InventoryStore.TryCapture(player.Handle.Id,
        out InventorySnapshot? inventory) || inventory is null) {
      return 0;
    }
    int selectedSlotIndex = player.Movement.SelectedItem;
    return selectedSlotIndex < 10
      ? ItemContentFacts.GetAxePower(inventory.Slots[selectedSlotIndex].Item.Type)
      : 0;
  }

  private static int CountTreeWoodBonuses(int treeTileCount, int axePower,
      ITreeHarvestRandomSource random) {
    int bonusWoodDropCount = 0;
    for (int index = 0; index < treeTileCount; index++) {
      if (random.Next(35) <= axePower || random.Next(3) == 0) {
        bonusWoodDropCount++;
      }
    }
    return bonusWoodDropCount;
  }

  private static bool TryGetBasicPlacementTile(int itemType, short requestedTile,
      out ushort tileType) {
    (int ItemType, ushort TileType) mapping = requestedTile switch {
      0 => (2, 0),
      1 => (3, 1),
      30 => (9, 30),
      _ => (-1, ushort.MaxValue)
    };
    tileType = mapping.TileType;
    return itemType == mapping.ItemType;
  }

  private static bool IsWithinStandardTileReach(PlayerEntity player, TileCoordinate coordinate) {
    return IsWithinTileReach(player, coordinate, StandardTileReachTiles);
  }

  private static bool IsWithinTreeHarvestReach(PlayerEntity player,
      TileCoordinate coordinate) {
    return IsWithinTileReach(player, coordinate, TreeHarvestReachTiles);
  }

  private static bool IsWithinTileReach(PlayerEntity player, TileCoordinate coordinate,
      int tileReach) {
    var position = player.Movement.Position;
    int width = player.RuntimeSpatial.Width;
    int height = player.RuntimeSpatial.Height;
    if (width <= 0 || height <= 0 || !float.IsFinite(position.X) ||
        !float.IsFinite(position.Y)) {
      return false;
    }

    int left = (int)(position.X / 16f) - tileReach;
    int right = (int)MathF.Ceiling((position.X + width) / 16f) - 1 + tileReach;
    int top = (int)(position.Y / 16f) - tileReach;
    int bottom = (int)MathF.Ceiling((position.Y + height) / 16f) - 1 + tileReach;
    return coordinate.X >= left && coordinate.X <= right &&
      coordinate.Y >= top && coordinate.Y <= bottom;
  }

  private AreaTileChangePacket CreateTileCorrection(TileCoordinate coordinate, WorldTile tile) {
    return new AreaTileChangePacket {
      StartX = checked((short)coordinate.X), StartY = checked((short)coordinate.Y),
      Width = 1, Height = 1,
      Tiles = [MechanismPacketProducer.CreateTileWireValue(tile, _composition.Version4WorldFacts)]
    };
  }

  private void AddTileCorrection(SessionEpoch target, TileCoordinate coordinate, WorldTile tile,
      CommitVersion version, List<OutboundPacketIntent> intents) {
    intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
      CreateTileCorrection(coordinate, tile), [target], null, version));
  }

  private void AddTileCorrections(SessionEpoch target, WorldSpatialSnapshot snapshot,
      IReadOnlyList<TileCoordinate> coordinates, CommitVersion version,
      List<OutboundPacketIntent> intents) {
    foreach (TileCoordinate coordinate in coordinates) {
      AddTileCorrection(target, coordinate, snapshot.GetTile(coordinate), version, intents);
    }
  }

  private void AddInventoryCorrection(SessionEpoch target, byte playerSlot,
      InventorySlotSnapshot slot, CommitVersion revision, List<OutboundPacketIntent> intents) {
    intents.Add(_composition.Version4WireProfile.CreateOutboundIntent(
      CreateInventoryCorrection(playerSlot, slot), [target], null, revision));
  }

  private static SyncEquipmentPacket CreateInventoryCorrection(byte playerSlot,
      InventorySlotSnapshot slot) {
    return new SyncEquipmentPacket {
      Player = playerSlot,
      Slot = slot.SlotId,
      Stack = checked((short)slot.Item.Stack),
      Prefix = slot.Item.Prefix,
      ItemType = checked((short)slot.Item.Type),
      Favorited = slot.Item.Favorited
    };
  }
}
