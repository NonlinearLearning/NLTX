using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Replication;

public sealed class WorldSectionReplication
{
  private const float MaximumClientViewDistance = 512.0f;
  private const int InitialSectionColumns = 5;
  private const int InitialSectionRows = 3;
  private readonly SectionVisibilitySelector _visibilitySelector = new();
  private readonly WorldGrid _world;

  public WorldSectionReplication(WorldGrid world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public WorldSpawnCoordinates GetInitialSpawnCoordinates()
  {
    return new WorldSpawnCoordinates(_world.Width / 2, _world.Height / 4);
  }

  public WorldSpawnCoordinates ResolvePlayerSpawn(PlayerSpawnPacket request)
  {
    if (request.SpawnX < 0 || request.SpawnY < 0)
    {
      return GetInitialSpawnCoordinates();
    }

    return new WorldSpawnCoordinates(request.SpawnX, request.SpawnY);
  }

  public IReadOnlyList<byte[]> CreateInitialWorldStream(
    SpawnTileDataRequestPacket request,
    SessionSectionVisibility visibility)
  {
    ArgumentNullException.ThrowIfNull(visibility);

    IReadOnlyList<WorldSectionSnapshot> snapshots = CollectInitialSections(request, visibility);
    return TerrariaPacketCodec.CreateInitialWorldStream(snapshots);
  }

  public IReadOnlyList<byte[]> CreateInitialWorldStream(
    SpawnTileDataRequestPacket request,
    SessionReplicationState state,
    IReadOnlyList<ChestSnapshot> chests)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(chests);

    IReadOnlyList<WorldSectionSnapshot> snapshots = CollectInitialSections(request, state);
    List<byte[]> frames = new(snapshots.Count + 2 + chests.Count * (ChestComponent.SlotCount + 1))
    {
      TerrariaPacketCodec.EncodeStatusTextSize(snapshots.Count, "Receiving tile data")
    };
    for (int snapshotIndex = 0; snapshotIndex < snapshots.Count; snapshotIndex++)
    {
      WorldSectionSnapshot snapshot = snapshots[snapshotIndex];
      frames.Add(TerrariaPacketCodec.Encode(snapshot, chests));
      AddChestContentsForSection(frames, snapshot.Coordinates, chests);
    }

    frames.Add(TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.InitialSpawn, Array.Empty<byte>())));
    return frames;
  }

  public IReadOnlyList<byte[]> CreateChangedWorldStream(SessionReplicationState state)
  {
    ArgumentNullException.ThrowIfNull(state);

    List<WorldSectionSnapshot> visibleSnapshots = new(state.VisibleSections.Count);
    foreach (WorldSectionCoordinates coordinates in state.VisibleSections)
    {
      visibleSnapshots.Add(_world.CreateSectionSnapshot(coordinates));
    }

    IReadOnlyList<WorldSectionSnapshot> changed = state.CollectChangedSections(visibleSnapshots);
    List<byte[]> frames = new(changed.Count);
    for (int index = 0; index < changed.Count; index++)
    {
      frames.Add(TerrariaPacketCodec.Encode(changed[index]));
    }

    return frames;
  }

  public IReadOnlyList<byte[]> CreateRequestedSectionStream(
    RequestSectionPacket request,
    SessionReplicationState state,
    IReadOnlyList<ChestSnapshot> chests)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(chests);
    if (!IsValidSection(request.SectionX, request.SectionY))
    {
      return [];
    }

    WorldSectionCoordinates coordinates = new(request.SectionX, request.SectionY);
    WorldSectionSnapshot snapshot = _world.CreateSectionSnapshot(coordinates);
    _ = state.CollectChangedSections([snapshot]);

    List<byte[]> frames = new(1 + chests.Count * (ChestComponent.SlotCount + 1));
    frames.Add(TerrariaPacketCodec.Encode(snapshot, chests));
    AddChestContentsForSection(frames, coordinates, chests);
    return frames;
  }

  public IReadOnlyList<byte[]> UpdatePlayerVisibility(
    PlayerSnapshot player,
    SessionReplicationState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (state.IsInitialVisibilityLocked)
    {
      return [];
    }

    SimulationVector viewPosition = player.Position;
    if (state.ClientViewPosition is SimulationVector clientPosition &&
        IsUsableClientViewPosition(clientPosition, player.Position))
    {
      viewPosition = clientPosition;
    }

    PlayerSnapshot viewPlayer = player with { Position = viewPosition };
    IReadOnlyList<WorldSectionCoordinates> visibleSections = _visibilitySelector.Select(
      _world,
      viewPlayer);
    List<WorldSectionSnapshot> snapshots = new(visibleSections.Count);
    for (int index = 0; index < visibleSections.Count; index++)
    {
      snapshots.Add(_world.CreateSectionSnapshot(visibleSections[index]));
    }

    IReadOnlyList<WorldSectionSnapshot> changed = state.ReplaceVisibleSections(snapshots);
    List<byte[]> frames = new(changed.Count);
    for (int index = 0; index < changed.Count; index++)
    {
      frames.Add(TerrariaPacketCodec.Encode(changed[index]));
    }

    return frames;
  }

  private bool IsUsableClientViewPosition(
    SimulationVector clientPosition,
    SimulationVector authoritativePosition)
  {
    return clientPosition.X >= 0.0f && clientPosition.X < _world.Width &&
      clientPosition.Y >= 0.0f && clientPosition.Y < _world.Height &&
      MathF.Abs(clientPosition.X - authoritativePosition.X) <= MaximumClientViewDistance &&
      MathF.Abs(clientPosition.Y - authoritativePosition.Y) <= MaximumClientViewDistance;
  }

  public IReadOnlyList<WorldSectionSnapshot> CollectInitialSections(
    SpawnTileDataRequestPacket request,
    SessionSectionVisibility visibility)
  {
    ArgumentNullException.ThrowIfNull(visibility);

    WorldSectionCoordinates center = GetWorldSpawnSectionCoordinates();
    List<WorldSectionSnapshot> snapshots = new(InitialSectionColumns * InitialSectionRows);
    int firstX = Clamp(center.X - InitialSectionColumns / 2, 0, SectionColumns - InitialSectionColumns);
    int firstY = Clamp(center.Y - InitialSectionRows / 2, 0, SectionRows - InitialSectionRows);
    for (int y = firstY; y < firstY + InitialSectionRows; y++)
    {
      for (int x = firstX; x < firstX + InitialSectionColumns; x++)
      {
        snapshots.Add(_world.CreateSectionSnapshot(new WorldSectionCoordinates(x, y)));
      }
    }

    return visibility.CollectChangedSections(snapshots);
  }

  public IReadOnlyList<WorldSectionSnapshot> CollectInitialSections(
    SpawnTileDataRequestPacket request,
    SessionReplicationState state)
  {
    ArgumentNullException.ThrowIfNull(state);

    WorldSectionCoordinates center = GetWorldSpawnSectionCoordinates();
    List<WorldSectionSnapshot> snapshots = CreateInitialSectionSnapshots(center);
    return state.CollectChangedSections(snapshots);
  }

  private List<WorldSectionSnapshot> CreateInitialSectionSnapshots(WorldSectionCoordinates center)
  {
    List<WorldSectionSnapshot> snapshots = new(InitialSectionColumns * InitialSectionRows);
    int firstX = Clamp(center.X - InitialSectionColumns / 2, 0, SectionColumns - InitialSectionColumns);
    int firstY = Clamp(center.Y - InitialSectionRows / 2, 0, SectionRows - InitialSectionRows);
    for (int y = firstY; y < firstY + InitialSectionRows; y++)
    {
      for (int x = firstX; x < firstX + InitialSectionColumns; x++)
      {
        snapshots.Add(_world.CreateSectionSnapshot(new WorldSectionCoordinates(x, y)));
      }
    }

    return snapshots;
  }

  private WorldSectionCoordinates GetWorldSpawnSectionCoordinates()
  {
    WorldSpawnCoordinates spawn = GetInitialSpawnCoordinates();
    return _world.GetSectionCoordinates(spawn.TileX, spawn.TileY);
  }

  private bool IsValidSection(int sectionX, int sectionY)
  {
    return sectionX >= 0 && sectionX < SectionColumns &&
      sectionY >= 0 && sectionY < SectionRows;
  }

  private static void AddChestContentsForSection(
    List<byte[]> frames,
    WorldSectionCoordinates section,
    IReadOnlyList<ChestSnapshot> chests)
  {
    for (int chestIndex = 0; chestIndex < chests.Count; chestIndex++)
    {
      ChestSnapshot chest = chests[chestIndex];
      if (chest.Section != section)
      {
        continue;
      }

      frames.Add(TerrariaPacketCodec.EncodeChestSize(chest.ChestId, ChestComponent.SlotCount));
      for (byte slot = 0; slot < ChestComponent.SlotCount; slot++)
      {
        frames.Add(TerrariaPacketCodec.EncodeChestItem(new ChestItemReplicationSnapshot(
          chest.ChestId,
          slot,
          chest.Slots[slot],
          Opener: null,
          Revision: chest.Revision)));
      }
    }
  }

  private int SectionColumns => _world.Width / WorldGrid.SectionWidth;
  private int SectionRows => _world.Height / WorldGrid.SectionHeight;

  private static int Clamp(int value, int minimum, int maximum)
  {
    return Math.Clamp(value, minimum, maximum);
  }
}
