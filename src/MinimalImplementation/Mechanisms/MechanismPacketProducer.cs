using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Encodes bounded P06 wire intents from committed or prepared P03 state.</summary>
public sealed class MechanismPacketProducer
{
  // Bounds packet 20 below the wire payload limit even when every optional tile field is set.
  private const int _tileBatchSize = 32;
  private readonly Version4WireProfile _wireProfile;
  private readonly WorldFacts _facts;

  public MechanismPacketProducer(Version4WireProfile wireProfile, WorldFacts facts)
  {
    ArgumentNullException.ThrowIfNull(wireProfile);
    ArgumentNullException.ThrowIfNull(facts);
    _wireProfile = wireProfile;
    _facts = facts;
  }

  public OutboundPacketIntent CreateHitSwitchIntent(
    HitSwitchPacket packet,
    IReadOnlyCollection<SessionEpoch> recipients,
    SessionEpoch ignoreEpoch,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(packet);
    Version4EncodedPacket encoded = _wireProfile.Encode(
      WireDirection.ServerToClient,
      packet);
    return new OutboundPacketIntent(
      recipients,
      ignoreEpoch,
      encoded.MessageId,
      encoded.Payload,
      commitVersion);
  }

  public OutboundPacketIntent CreateTileChangeIntent(
    WorldSnapshot world,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    return CreateTileChangeIntent(world, changedTiles, recipients, commitVersion, null);
  }

  /// <summary>
  /// Encodes before publication, using P03's normalized prepared tiles over the base snapshot.
  /// The temporary overlay is scoped to this call and is never authoritative world storage.
  /// </summary>
  public IReadOnlyList<OutboundPacketIntent> CreateTileChangeIntents(
    WorldSnapshot before,
    WorldTileMutationPreparation preparation,
    IReadOnlyCollection<SessionEpoch> recipients)
  {
    return CreateTileChangeIntents(
      before,
      preparation,
      recipients,
      Array.Empty<WorldSectionBounds>());
  }

  /// <summary>
  /// Encodes changed tiles and any required complete tile-square refreshes before publication.
  /// Required squares may contain unchanged tiles, whose values are read from the base snapshot.
  /// </summary>
  public IReadOnlyList<OutboundPacketIntent> CreateTileChangeIntents(
    WorldSnapshot before,
    WorldTileMutationPreparation preparation,
    IReadOnlyCollection<SessionEpoch> recipients,
    IReadOnlyList<WorldSectionBounds> tileSquareUpdates)
  {
    return CreateTileChangeIntents(
      before, preparation, recipients, tileSquareUpdates, tileSquareEffects: null);
  }

  /// <summary>Pre-encodes device refreshes and animations in traversal order.</summary>
  public IReadOnlyList<OutboundPacketIntent> CreateWiringIntents(
    WorldSnapshot before,
    WorldTileMutationPreparation preparation,
    IReadOnlyCollection<SessionEpoch> recipients,
    IReadOnlyList<WiringTileSquareEffect> tileSquareEffects)
  {
    ArgumentNullException.ThrowIfNull(tileSquareEffects);
    return CreateTileChangeIntents(before, preparation, recipients,
      tileSquareEffects.Select(effect => effect.Bounds).ToArray(), tileSquareEffects);
  }

  private IReadOnlyList<OutboundPacketIntent> CreateTileChangeIntents(
    WorldSnapshot before,
    WorldTileMutationPreparation preparation,
    IReadOnlyCollection<SessionEpoch> recipients,
    IReadOnlyList<WorldSectionBounds> tileSquareUpdates,
    IReadOnlyList<WiringTileSquareEffect>? tileSquareEffects)
  {
    ArgumentNullException.ThrowIfNull(before);
    ArgumentNullException.ThrowIfNull(preparation);
    ArgumentNullException.ThrowIfNull(recipients);
    ArgumentNullException.ThrowIfNull(tileSquareUpdates);
    if (preparation.WorldEpoch != before.WorldEpoch || preparation.BaseVersion != before.Version ||
        preparation.Result.Version is not CommitVersion version)
    {
      throw new ArgumentException("The prepared tiles require their matching base snapshot.",
        nameof(preparation));
    }

    Dictionary<TileCoordinate, WorldTile> overlay = preparation.Tiles.ToDictionary(
      pair => pair.Key, pair => pair.Value);
    var intents = new List<OutboundPacketIntent>();
    var requiredTiles = new HashSet<TileCoordinate>();
    var requiredSquares = new List<TileCoordinate[]>();
    foreach (WorldSectionBounds bounds in tileSquareUpdates.Distinct())
    {
      TileCoordinate[] square = GetTileSquareCoordinates(before, bounds);
      requiredSquares.Add(square);
      requiredTiles.UnionWith(square);
    }
    TileCoordinate[] unframedChanges = overlay.Keys
      .Where(coordinate => !requiredTiles.Contains(coordinate))
      .ToArray();
    foreach (var batch in unframedChanges
        .GroupBy(coordinate => (X: coordinate.X / _tileBatchSize, Y: coordinate.Y / _tileBatchSize))
        .OrderBy(group => group.Key.Y)
        .ThenBy(group => group.Key.X))
    {
      intents.Add(CreateTileChangeIntent(before, batch.ToArray(), recipients, version, overlay));
    }
    if (tileSquareEffects is null)
    {
      foreach (TileCoordinate[] square in requiredSquares)
      {
        intents.Add(CreateTileChangeIntent(before, square, recipients, version, overlay));
      }
    }
    else
    {
      var lastSentTiles = new Dictionary<TileCoordinate, WorldTile>();
      foreach (WiringTileSquareEffect effect in tileSquareEffects)
      {
        intents.Add(CreateTileChangeIntent(before, effect.Tiles.Keys.ToArray(),
          recipients, version, effect.Tiles));
        foreach ((TileCoordinate coordinate, WorldTile tile) in effect.Tiles)
        {
          lastSentTiles[coordinate] = tile;
        }
        if (effect.AnimationType is short animationType)
        {
          var packet = new TemporaryAnimationPacket
          {
            AnimationType = animationType,
            TileType = effect.AnimationTileType,
            X = checked((short)effect.Bounds.OriginX),
            Y = checked((short)effect.Bounds.OriginY)
          };
          for (int send = 0; send < effect.AnimationSendCount; send++)
          {
            intents.Add(CreateTemporaryAnimationIntent(packet, recipients, version));
          }
        }
      }
      // Later work, such as pump transfer, may change a previously refreshed footprint.
      TileCoordinate[] remaining = lastSentTiles.Where(pair =>
        pair.Value != (overlay.TryGetValue(pair.Key, out WorldTile tile)
          ? tile : before.GetTile(pair.Key))).Select(pair => pair.Key).ToArray();
      foreach (var batch in remaining
          .GroupBy(coordinate => (X: coordinate.X / _tileBatchSize, Y: coordinate.Y / _tileBatchSize))
          .OrderBy(group => group.Key.Y)
          .ThenBy(group => group.Key.X))
      {
        intents.Add(CreateTileChangeIntent(before, batch.ToArray(), recipients, version, overlay));
      }
    }
    return Array.AsReadOnly(intents.ToArray());
  }

  private OutboundPacketIntent CreateTemporaryAnimationIntent(
    TemporaryAnimationPacket packet,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion)
  {
    Version4EncodedPacket encoded = _wireProfile.Encode(WireDirection.ServerToClient, packet);
    return new OutboundPacketIntent(recipients, ignoreEpoch: null,
      encoded.MessageId, encoded.Payload, commitVersion);
  }

  private static TileCoordinate[] GetTileSquareCoordinates(
    WorldSnapshot world,
    WorldSectionBounds bounds)
  {
    long right = (long)bounds.OriginX + bounds.Width;
    long bottom = (long)bounds.OriginY + bounds.Height;
    if (bounds.Width <= 0 || bounds.Height <= 0 ||
        bounds.Width > byte.MaxValue || bounds.Height > byte.MaxValue ||
        bounds.OriginX < 0 || bounds.OriginY < 0 ||
        right > world.Header.Geometry.Width || bottom > world.Header.Geometry.Height)
    {
      throw new InvalidOperationException(
        "A required tile-square refresh is outside the Version4 packet bounds.");
    }

    var coordinates = new TileCoordinate[checked(bounds.Width * bounds.Height)];
    int index = 0;
    for (int x = 0; x < bounds.Width; x++)
    {
      for (int y = 0; y < bounds.Height; y++)
      {
        coordinates[index++] = new TileCoordinate(bounds.OriginX + x, bounds.OriginY + y);
      }
    }
    return coordinates;
  }

  private OutboundPacketIntent CreateTileChangeIntent(
    WorldSnapshot world,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion commitVersion,
    IReadOnlyDictionary<TileCoordinate, WorldTile>? overlay)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(changedTiles);
    if (changedTiles.Count == 0)
    {
      throw new ArgumentException(
        "A tile-change intent requires at least one changed tile.",
        nameof(changedTiles));
    }

    WorldSectionBounds bounds = GetChangedBounds(world, changedTiles);
    var packet = new AreaTileChangePacket
    {
      StartX = checked((short)bounds.OriginX),
      StartY = checked((short)bounds.OriginY),
      Width = checked((byte)bounds.Width),
      Height = checked((byte)bounds.Height),
      ChangeType = 0,
      Tiles = CreateTiles(world, bounds, overlay)
    };
    Version4EncodedPacket encoded = _wireProfile.Encode(
      WireDirection.ServerToClient,
      packet);
    return new OutboundPacketIntent(
      recipients,
      ignoreEpoch: null,
      encoded.MessageId,
      encoded.Payload,
      commitVersion);
  }

  private static WorldSectionBounds GetChangedBounds(
    WorldSnapshot world,
    IReadOnlyCollection<TileCoordinate> changedTiles)
  {
    int minX = changedTiles.Min(coordinate => coordinate.X);
    int maxX = changedTiles.Max(coordinate => coordinate.X);
    int minY = changedTiles.Min(coordinate => coordinate.Y);
    int maxY = changedTiles.Max(coordinate => coordinate.Y);
    if (!world.Header.Geometry.Contains(new TileCoordinate(minX, minY)) ||
        !world.Header.Geometry.Contains(new TileCoordinate(maxX, maxY)) ||
        maxX - minX + 1 > byte.MaxValue ||
        maxY - minY + 1 > byte.MaxValue)
    {
      throw new InvalidOperationException(
        "The changed tile rectangle cannot be represented by Version4 packet 20.");
    }

    return new WorldSectionBounds(
      minX,
      minY,
      maxX - minX + 1,
      maxY - minY + 1);
  }

  private Packet20Tile[] CreateTiles(
    WorldSnapshot world,
    WorldSectionBounds bounds,
    IReadOnlyDictionary<TileCoordinate, WorldTile>? overlay)
  {
    var tiles = new Packet20Tile[checked(bounds.Width * bounds.Height)];
    int index = 0;
    for (int x = 0; x < bounds.Width; x++)
    {
      for (int y = 0; y < bounds.Height; y++)
      {
        var coordinate = new TileCoordinate(
          bounds.OriginX + x,
          bounds.OriginY + y);
        WorldTile tile = overlay is not null && overlay.TryGetValue(coordinate, out WorldTile staged)
          ? staged : world.GetTile(coordinate);
        tiles[index++] = CreateTileWireValue(tile, _facts);
      }
    }

    return tiles;
  }

  /// <summary>Projects one captured tile for mechanism output or terrain correction.</summary>
  public static Packet20Tile CreateTileWireValue(WorldTile tile, WorldFacts facts) {
    return new Packet20Tile {
          Active = tile.Active,
          TileType = tile.Type,
          FrameX = tile.FrameX,
          FrameY = tile.FrameY,
          TileColor = tile.TileColor,
          TileColorPresent = tile.Active && tile.TileColor > 0,
          Wall = tile.Wall,
          WallColor = tile.WallColor,
          WallColorPresent = tile.Wall > 0 && tile.WallColor > 0,
          LiquidAmount = tile.LiquidAmount,
          LiquidType = (byte)tile.LiquidType,
          LiquidPresent = tile.LiquidAmount > 0,
          FramePresent = tile.Active && tile.Type < facts.FrameImportant.Count &&
            facts.FrameImportant[tile.Type],
          Wire = (tile.Wires & TileWireMask.Red) != 0,
          HalfBrick = tile.HalfBlock,
          Actuator = tile.Actuator,
          Inactive = tile.Inactive,
          Wire2 = (tile.Wires & TileWireMask.Blue) != 0,
          Wire3 = (tile.Wires & TileWireMask.Green) != 0,
          Slope = tile.Slope,
          Wire4 = (tile.Wires & TileWireMask.Yellow) != 0,
          FullbrightBlock = (tile.TileHeader3 & 0x80) != 0,
          FullbrightWall = (tile.WallHeader & 0x80) != 0,
          InvisibleBlock = (tile.TileHeader3 & 0x20) != 0,
          InvisibleWall = (tile.TileHeader3 & 0x40) != 0
    };
  }
}
