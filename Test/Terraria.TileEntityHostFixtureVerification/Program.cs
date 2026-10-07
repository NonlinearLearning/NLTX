using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldInteraction.TileEntities;
using Terraria.WorldStorage;

internal static class Program
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort TrainingDummyTileType = 378;
  private const ushort LogicSensorTileType = 423;
  private const byte LogicSensorPlayerAbove = (byte)LogicCheckType.PlayerAbove;
  private const short LogicOnFrameX = 18;
  private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

  private static int Main(string[] args)
  {
    try
    {
      if (args.Length == 4 && string.Equals(args[0], "prepare", StringComparison.Ordinal))
      {
        Prepare(args[1], args[2], args[3]);
        return 0;
      }

      if (args.Length == 4 && string.Equals(args[0], "verify", StringComparison.Ordinal))
      {
        Verify(args[1], args[2], args[3]);
        return 0;
      }

      throw new ArgumentException(
        "Usage: prepare <source.wld> <fixture.wld> <manifest.json> | " +
        "verify <saved.wld> <manifest.json> <stage>");
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void Prepare(string sourcePath, string fixturePath, string manifestPath)
  {
    string source = Path.GetFullPath(sourcePath);
    string fixture = Path.GetFullPath(fixturePath);
    string manifest = Path.GetFullPath(manifestPath);
    if (string.Equals(source, fixture, StringComparison.OrdinalIgnoreCase))
    {
      throw new InvalidOperationException("The fixture path must differ from the source world.");
    }
    if (!File.Exists(source))
    {
      throw new FileNotFoundException("The source world file does not exist.", source);
    }
    if (File.Exists(fixture) || File.Exists(manifest))
    {
      throw new IOException("The fixture or manifest output already exists.");
    }

    byte[] sourceBytes = File.ReadAllBytes(source);
    WorldPersistenceDocument document = Decode(sourceBytes, source);
    WorldFileEnvironmentSection environment = GetRequired<WorldFileEnvironmentSection>(
      document,
      WorldFileEnvironmentSection.SectionId);
    WorldFileTilePayloadSection tilePayload = GetRequired<WorldFileTilePayloadSection>(
      document,
      WorldFileTilePayloadSection.SectionId);
    WorldFileTileEntitySection tileEntitySection = GetOptionalTileEntities(document);
    TileMapSnapshot sourceTiles = new WorldFileTileCodec().Decode(tilePayload);
    var tileEntities = new WorldFileTileEntityCodec().Decode(tileEntitySection);
    TileCellState[] tiles = CopyTiles(sourceTiles);

    foreach (TileEntitySnapshot entity in tileEntities)
    {
      if (!IsInBounds(entity.Anchor, sourceTiles.Width, sourceTiles.Height))
      {
        throw new InvalidDataException(
          $"Source TileEntity {entity.Id.Value} has an out-of-bounds anchor.");
      }

      ClearAnchor(tiles, sourceTiles.Height, entity.Anchor);
    }

    var reservedAnchors = new HashSet<TileCoordinate>();
    int startX = Math.Clamp(environment.SpawnTileX, 1, sourceTiles.Width - 2);
    int startY = Math.Clamp(environment.SpawnTileY + 2, 1, sourceTiles.Height - 2);
    TileCoordinate dummyAnchor = FindInactiveAnchor(
      tiles,
      sourceTiles.Width,
      sourceTiles.Height,
      startX,
      startY,
      reservedAnchors);
    reservedAnchors.Add(dummyAnchor);
    TileCoordinate sensorAnchor = FindInactiveAnchor(
      tiles,
      sourceTiles.Width,
      sourceTiles.Height,
      startX,
      startY,
      reservedAnchors);

    RequireFrameImportant(tilePayload.FrameImportant, TrainingDummyTileType);
    RequireFrameImportant(tilePayload.FrameImportant, LogicSensorTileType);
    SetAnchorTile(tiles, sourceTiles.Height, dummyAnchor, TrainingDummyTileType, frameX: 0);
    SetAnchorTile(tiles, sourceTiles.Height, sensorAnchor, LogicSensorTileType, LogicOnFrameX);

    var dummy = new TileEntitySnapshot(
      new TileEntityId(0),
      new TileEntityTypeId(0),
      dummyAnchor,
      Array.Empty<ItemState>());
    var sensor = new TileEntitySnapshot(
      new TileEntityId(1),
      new TileEntityTypeId(2),
      sensorAnchor,
      Array.Empty<ItemState>(),
      logicCheck: LogicSensorPlayerAbove,
      logicOn: true);
    var fixtureTiles = new TileMapSnapshot(
      sourceTiles.Width,
      sourceTiles.Height,
      tiles);
    var fixtureEntities = new[] { dummy, sensor };
    WorldPersistenceDocument fixtureDocument = document.WithReplacements(
      WorldPersistenceSection.Create(
        WorldFileTilePayloadSection.SectionId,
        new WorldFileTileCodec().Encode(fixtureTiles, tilePayload.FrameImportant)),
      WorldPersistenceSection.Create(
        WorldFileTileEntitySection.SectionId,
        new WorldFileTileEntityCodec().Encode(fixtureEntities)),
      WorldPersistenceSection.Create(
        WorldFileNpcSection.SectionId,
        WorldFileNpcSection.Empty));

    WorldStorageFailure validation = new WorldFileDocumentValidator().Validate(fixtureDocument);
    if (validation.Kind != WorldStorageFailureKind.None)
    {
      throw new InvalidDataException(
        $"The generated TileEntity fixture is invalid: {validation.Kind}: {validation.Detail}");
    }

    WorldSaveEncodeResult encoded = new WorldFileDocumentEncoder().Encode(fixtureDocument);
    if (!encoded.Succeeded)
    {
      throw new InvalidDataException(
        $"The generated TileEntity fixture could not be encoded: " +
        $"{encoded.Failure.Kind}: {encoded.Failure.Detail}");
    }

    Directory.CreateDirectory(Path.GetDirectoryName(fixture)!);
    Directory.CreateDirectory(Path.GetDirectoryName(manifest)!);
    File.WriteAllBytes(fixture, encoded.Bytes.ToArray());
    var manifestDocument = new
    {
      SourceWorldSha256 = Convert.ToHexString(SHA256.HashData(sourceBytes)),
      FixtureWorldSha256 = HashFile(fixture),
      ExpectedTileEntityUpdatePassCount = fixtureEntities.Length,
      TrainingDummy = new
      {
        Type = 0,
        X = dummyAnchor.X,
        Y = dummyAnchor.Y,
      },
      LogicSensor = new
      {
        Type = 2,
        X = sensorAnchor.X,
        Y = sensorAnchor.Y,
        LogicCheck = LogicSensorPlayerAbove,
        InitialLogicOn = true,
        ExpectedLogicOnAfterTick = false,
      },
      InputNpcRecordCount = 0,
    };
    File.WriteAllText(manifest, JsonSerializer.Serialize(manifestDocument, JsonOptions));
    Verify(fixture, manifest, "prepared");
  }

  private static void Verify(string worldPath, string manifestPath, string stage)
  {
    using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
    JsonElement root = manifest.RootElement;
    FixtureEntity dummyExpected = ReadFixtureEntity(root.GetProperty("TrainingDummy"));
    FixtureEntity sensorExpected = ReadFixtureEntity(root.GetProperty("LogicSensor"));
    string fullWorldPath = Path.GetFullPath(worldPath);
    WorldPersistenceDocument document = Decode(File.ReadAllBytes(fullWorldPath), fullWorldPath);
    WorldFileTileEntitySection tileEntitySection = GetRequired<WorldFileTileEntitySection>(
      document,
      WorldFileTileEntitySection.SectionId);
    IReadOnlyList<TileEntitySnapshot> entities = new WorldFileTileEntityCodec()
      .Decode(tileEntitySection);
    int expectedCount = root.GetProperty("ExpectedTileEntityUpdatePassCount").GetInt32();
    if (entities.Count != expectedCount)
    {
      throw new InvalidDataException(
        $"TileEntity fixture stage '{stage}' has {entities.Count} entries; " +
        $"expected {expectedCount}.");
    }

    TileMapSnapshot tiles = new WorldFileTileCodec().Decode(
      GetRequired<WorldFileTilePayloadSection>(document, WorldFileTilePayloadSection.SectionId));
    TileEntitySnapshot dummy = RequireEntity(entities, dummyExpected);
    TileEntitySnapshot sensor = RequireEntity(entities, sensorExpected);
    bool prepared = string.Equals(stage, "prepared", StringComparison.Ordinal);

    if (sensor.LogicCheck != LogicSensorPlayerAbove || sensor.LogicOn != prepared)
    {
      throw new InvalidDataException(
        $"TileEntity fixture stage '{stage}' has unexpected Logic Sensor state.");
    }
    if (prepared ? dummy.NpcIndex != -1 : dummy.NpcIndex < 0)
    {
      throw new InvalidDataException(
        $"TileEntity fixture stage '{stage}' has an unexpected Training Dummy NPC index.");
    }

    TileCellState dummyTile = tiles.GetTile(dummy.Anchor.X, dummy.Anchor.Y);
    TileCellState sensorTile = tiles.GetTile(sensor.Anchor.X, sensor.Anchor.Y);
    RequireActiveAnchor(dummyTile, TrainingDummyTileType, dummy.Anchor);
    RequireActiveAnchor(sensorTile, LogicSensorTileType, sensor.Anchor);
    short expectedSensorFrameX = prepared && sensor.LogicOn ? LogicOnFrameX : (short)0;
    if (sensorTile.FrameX != expectedSensorFrameX)
    {
      throw new InvalidDataException(
        $"TileEntity fixture stage '{stage}' has an unexpected Logic Sensor frame.");
    }

    object report = new
    {
      Succeeded = true,
      Stage = stage,
      WorldPath = fullWorldPath,
      WorldSha256 = HashFile(fullWorldPath),
      TileEntityCount = entities.Count,
      TrainingDummy = new
      {
        Anchor = new { dummy.Anchor.X, dummy.Anchor.Y },
        dummy.NpcIndex,
      },
      LogicSensor = new
      {
        Anchor = new { sensor.Anchor.X, sensor.Anchor.Y },
        LogicCheck = sensor.LogicCheck,
        sensor.LogicOn,
        sensorTile.FrameX,
      },
    };
    Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions));
  }

  private static FixtureEntity ReadFixtureEntity(JsonElement element)
  {
    return new FixtureEntity(
      element.GetProperty("Type").GetInt32(),
      element.GetProperty("X").GetInt32(),
      element.GetProperty("Y").GetInt32());
  }

  private static TileEntitySnapshot RequireEntity(
    IReadOnlyList<TileEntitySnapshot> entities,
    FixtureEntity expected)
  {
    var anchor = new TileCoordinate(expected.X, expected.Y);
    TileEntitySnapshot? entity = entities.SingleOrDefault(
      candidate => candidate.Anchor == anchor && candidate.Type.Value == expected.Type);
    return entity ?? throw new InvalidDataException(
      $"TileEntity type {expected.Type} at ({expected.X}, {expected.Y}) is missing.");
  }

  private static void RequireActiveAnchor(
    TileCellState tile,
    ushort expectedType,
    TileCoordinate anchor)
  {
    if ((tile.TileHeader & ActiveTileFlag) == 0 || tile.Type != expectedType)
    {
      throw new InvalidDataException(
        $"TileEntity anchor ({anchor.X}, {anchor.Y}) is not active type {expectedType}.");
    }
  }

  private static WorldPersistenceDocument Decode(byte[] bytes, string path)
  {
    WorldPersistenceDecodeResult decoded = new WorldFileDocumentDecoder().Decode(bytes);
    if (!decoded.Succeeded || decoded.Document is null)
    {
      throw new InvalidDataException(
        $"WorldFile '{path}' could not be decoded: " +
        $"{decoded.Failure.Kind}: {decoded.Failure.Detail}");
    }

    return decoded.Document;
  }

  private static TSection GetRequired<TSection>(
    WorldPersistenceDocument document,
    string sectionId)
    where TSection : notnull
  {
    if (!document.TryGetSection<TSection>(sectionId, out WorldLoadSection<TSection> section) ||
        !section.IsPresent)
    {
      throw new InvalidDataException($"WorldFile section '{sectionId}' is missing.");
    }

    return section.Value;
  }

  private static WorldFileTileEntitySection GetOptionalTileEntities(
    WorldPersistenceDocument document)
  {
    if (document.TryGetSection<WorldFileTileEntitySection>(
          WorldFileTileEntitySection.SectionId,
          out WorldLoadSection<WorldFileTileEntitySection> section) && section.IsPresent)
    {
      return section.Value;
    }

    return new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty);
  }

  private static TileCellState[] CopyTiles(TileMapSnapshot snapshot)
  {
    var tiles = new TileCellState[checked(snapshot.Width * snapshot.Height)];
    for (int x = 0; x < snapshot.Width; x++)
    {
      for (int y = 0; y < snapshot.Height; y++)
      {
        tiles[x * snapshot.Height + y] = snapshot.GetTile(x, y);
      }
    }

    return tiles;
  }

  private static TileCoordinate FindInactiveAnchor(
    TileCellState[] tiles,
    int width,
    int height,
    int startX,
    int startY,
    IReadOnlySet<TileCoordinate> reservedAnchors)
  {
    for (int radius = 0; radius < width + height; radius++)
    {
      for (int offsetX = -radius; offsetX <= radius; offsetX++)
      {
        int offsetY = radius - Math.Abs(offsetX);
        var candidate = new TileCoordinate(startX + offsetX, startY + offsetY);
        if (IsInactive(candidate, tiles, width, height, reservedAnchors))
        {
          return candidate;
        }

        if (offsetY > 0)
        {
          candidate = new TileCoordinate(startX + offsetX, startY - offsetY);
          if (IsInactive(candidate, tiles, width, height, reservedAnchors))
          {
            return candidate;
          }
        }
      }
    }

    throw new InvalidDataException(
      "The world has no inactive tile for a TileEntity fixture anchor.");
  }

  private static bool IsInactive(
    TileCoordinate candidate,
    TileCellState[] tiles,
    int width,
    int height,
    IReadOnlySet<TileCoordinate> reservedAnchors)
  {
    if (!IsInBounds(candidate, width, height) || reservedAnchors.Contains(candidate))
    {
      return false;
    }

    TileCellState tile = tiles[candidate.X * height + candidate.Y];
    return (tile.TileHeader & ActiveTileFlag) == 0;
  }

  private static bool IsInBounds(TileCoordinate coordinate, int width, int height)
  {
    return (uint)coordinate.X < (uint)width && (uint)coordinate.Y < (uint)height;
  }

  private static void ClearAnchor(
    TileCellState[] tiles,
    int height,
    TileCoordinate anchor)
  {
    int index = anchor.X * height + anchor.Y;
    TileCellState tile = tiles[index];
    tile.TileHeader = 0;
    tile.Type = 0;
    tile.FrameX = 0;
    tile.FrameY = 0;
    tiles[index] = tile;
  }

  private static void SetAnchorTile(
    TileCellState[] tiles,
    int height,
    TileCoordinate anchor,
    ushort tileType,
    short frameX)
  {
    int index = anchor.X * height + anchor.Y;
    TileCellState tile = tiles[index];
    tile.TileHeader = ActiveTileFlag;
    tile.Type = tileType;
    tile.FrameX = frameX;
    tile.FrameY = 0;
    tiles[index] = tile;
  }

  private static void RequireFrameImportant(IReadOnlyList<bool> frameImportant, ushort tileType)
  {
    if (tileType >= frameImportant.Count || !frameImportant[tileType])
    {
      throw new InvalidDataException(
        $"TileEntity fixture anchor type {tileType} is not frame-important in the WorldFile.");
    }
  }

  private static string HashFile(string path)
  {
    return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
  }

  private readonly record struct FixtureEntity(int Type, int X, int Y);
}
