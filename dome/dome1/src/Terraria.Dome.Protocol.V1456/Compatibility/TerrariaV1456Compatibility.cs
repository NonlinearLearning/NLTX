using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public static class TerrariaV1456Compatibility
{
  private const byte PlayerControlsVelocityBit = 1 << 2;
  private const byte PlayerControlsMountBit = 1 << 7;
  private const byte PlayerControlsReturnPositionsBit = 1 << 6;
  private const byte PlayerControlsCameraTargetBit = 1 << 5;
  private const int PlayerControlsFixedPayloadLength = 14;
  private const int VectorLength = sizeof(float) * 2;

  // Terraria 1.4.5.6 reads frame coordinates for these tile types in the
  // compressed world stream.
  private static readonly HashSet<ushort> FrameImportantTileTypes =
  [
    3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 26, 27,
    28, 29, 31, 33, 34, 35, 36, 42, 49, 50, 55, 61, 71, 72, 73, 74, 77,
    78, 79, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95,
    96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 125,
    126, 128, 129, 132, 133, 134, 135, 136, 137, 138, 139, 141, 142, 143,
    144, 149, 165, 171, 172, 173, 174, 178, 184, 185, 186, 187, 201, 207,
    209, 210, 212, 215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 235,
    236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269,
    270, 271, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287,
    288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301,
    302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319,
    320, 323, 324, 334, 335, 337, 338, 339, 349, 354, 355, 356, 358, 359,
    360, 361, 362, 363, 364, 372, 373, 374, 375, 376, 377, 378, 380, 386,
    387, 388, 389, 390, 391, 392, 393, 394, 395, 405, 406, 410, 411, 412,
    413, 414, 419, 420, 423, 424, 425, 427, 428, 429, 440, 441, 442, 443,
    444, 445, 452, 453, 454, 455, 456, 457, 461, 462, 463, 464, 465, 466,
    467, 468, 469, 470, 471, 475, 476, 480, 484, 485, 486, 487, 488, 489,
    490, 491, 493, 494, 497, 499, 505, 506, 509, 510, 511, 518, 519, 520,
    521, 522, 523, 524, 525, 526, 527, 529, 530, 531, 532, 533, 538, 542,
    543, 544, 545, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 558,
    559, 560, 564, 565, 567, 568, 569, 570, 571, 572, 573, 579, 580, 581,
    582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595,
    596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609,
    610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 621, 622, 623, 624,
    629, 630, 631, 632, 634, 637, 639, 640, 642, 643, 644, 645, 646, 653,
    654, 656, 657, 658, 660, 663, 664, 665, 695, 696, 698, 699, 700, 701,
    702, 703, 704, 705, 707, 709, 710, 711, 712, 713, 714, 715, 716, 720,
    721, 723, 724, 725, 726, 733, 751, 752
  ];

  public static LegacyPlayerControlsProjection DecodePlayerControls(
    ReadOnlySpan<byte> payload)
  {
    if (payload.Length < PlayerControlsFixedPayloadLength)
    {
      throw new InvalidDataException(
        "Terraria PlayerControls packet is shorter than its required fields.");
    }

    using MemoryStream stream = new(payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    byte playerSlot = reader.ReadByte();
    byte controlFlags = reader.ReadByte();
    byte secondaryFlags = reader.ReadByte();
    byte tertiaryFlags = reader.ReadByte();
    byte quaternaryFlags = reader.ReadByte();
    byte selectedItem = reader.ReadByte();
    SimulationVector position = ReadVector(reader);
    SimulationVector? velocity = null;
    ushort? mountType = null;
    SimulationVector? returnOrigin = null;
    SimulationVector? returnHome = null;
    SimulationVector? cameraTarget = null;

    if ((secondaryFlags & PlayerControlsVelocityBit) != 0)
    {
      RequireRemaining(stream, VectorLength, "velocity");
      velocity = ReadVector(reader);
    }

    if ((secondaryFlags & PlayerControlsMountBit) != 0)
    {
      RequireRemaining(stream, sizeof(ushort), "mount");
      mountType = reader.ReadUInt16();
    }

    if ((tertiaryFlags & PlayerControlsReturnPositionsBit) != 0)
    {
      RequireRemaining(stream, VectorLength * 2, "return-position");
      returnOrigin = ReadVector(reader);
      returnHome = ReadVector(reader);
    }

    if ((quaternaryFlags & PlayerControlsCameraTargetBit) != 0)
    {
      RequireRemaining(stream, VectorLength, "camera-target");
      cameraTarget = ReadVector(reader);
    }

    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria PlayerControls packet has trailing data.");
    }

    return LegacyPlayerControlsProjection.Create(new LegacyPlayerControlsState(
      playerSlot,
      controlFlags,
      secondaryFlags,
      tertiaryFlags,
      quaternaryFlags,
      selectedItem,
      position,
      velocity,
      mountType,
      returnOrigin,
      returnHome,
      cameraTarget));
  }

  public static byte[] CreateWorldDataPayload(LegacyWorldDataContext context)
  {
    if (context.WorldName is null)
    {
      throw new ArgumentException("WorldData requires a world name.", nameof(context));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      WriteWorldData(writer, context);
    }

    return payload.ToArray();
  }

  public static byte[] EncodeWorldData(LegacyWorldDataContext context)
  {
    return EncodeFrame(TerrariaMessageId.WorldData, CreateWorldDataPayload(context));
  }

  public static IReadOnlyList<byte[]> CreateJoinGreetingFrames(
    LegacyWorldDataContext context,
    string playerName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(playerName);
    return
    [
      EncodeJoinWorldGreeting(context.WorldName),
      EncodeJoinPlayerGreeting(playerName)
    ];
  }

  public static byte[] EncodeTileSection(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests)
  {
    return EncodeTileSection(snapshot, chests, []);
  }

  public static byte[] EncodeTileSection(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests,
    IReadOnlyList<SignReplicationSnapshot> signs)
  {
    return EncodeTileSection(snapshot, chests, signs, []);
  }

  public static byte[] EncodeTileSection(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests,
    IReadOnlyList<SignReplicationSnapshot> signs,
    IReadOnlyList<LegacyTileEntity> tileEntities)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);

    using MemoryStream payload = new();
    using (DeflateStream compressor = new(payload, CompressionMode.Compress, leaveOpen: true))
    using (BinaryWriter writer = new(compressor, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(snapshot.Coordinates.X * WorldGrid.SectionWidth);
      writer.Write(snapshot.Coordinates.Y * WorldGrid.SectionHeight);
      writer.Write((short)snapshot.Width);
      writer.Write((short)snapshot.Height);
      WriteTileSectionTiles(writer, snapshot);
      WriteSectionChests(writer, snapshot.Coordinates, chests);
      WriteSectionSigns(writer, snapshot.Coordinates, signs);
      WriteTileEntities(writer, tileEntities);
    }

    return EncodeFrame(TerrariaMessageId.TileSection, payload.ToArray());
  }

  public static byte[] EncodeTileSection(
    int originX,
    int originY,
    short width,
    short height,
    IReadOnlyList<LegacyTileSectionTile> tiles)
  {
    return EncodeTileSection(originX, originY, width, height, tiles, []);
  }

  public static byte[] EncodeTileSection(
    int originX,
    int originY,
    short width,
    short height,
    IReadOnlyList<LegacyTileSectionTile> tiles,
    IReadOnlyList<LegacyTileEntity> tileEntities)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(tileEntities);
    if (width <= 0 || height <= 0 || tiles.Count != width * height)
    {
      throw new ArgumentOutOfRangeException(nameof(tiles));
    }

    using MemoryStream payload = new();
    using (DeflateStream compressor = new(payload, CompressionMode.Compress, leaveOpen: true))
    using (BinaryWriter writer = new(compressor, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(originX);
      writer.Write(originY);
      writer.Write(width);
      writer.Write(height);
      WriteTileSectionTiles(writer, tiles);
      writer.Write((short)0);
      writer.Write((short)0);
      WriteTileEntities(writer, tileEntities);
    }

    return EncodeFrame(TerrariaMessageId.TileSection, payload.ToArray());
  }

  public static byte[] EncodeTileSquare(
    WorldGrid world,
    int x,
    int y,
    byte width,
    byte height,
    byte changeType)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (width == 0 || height == 0 || x < 0 || y < 0 ||
        x + width > world.Width || y + height > world.Height ||
        x > short.MaxValue || y > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)x);
      writer.Write((short)y);
      writer.Write(width);
      writer.Write(height);
      writer.Write(changeType);
      for (int xOffset = 0; xOffset < width; xOffset++)
      {
        for (int yOffset = 0; yOffset < height; yOffset++)
        {
          WriteTileSquareTile(writer, world.GetTile(x + xOffset, y + yOffset));
        }
      }
    }

    return EncodeFrame(TerrariaMessageId.TileSquare, payload.ToArray());
  }

  public static byte[] EncodeTileSquare(
    short x,
    short y,
    byte width,
    byte height,
    byte changeType,
    IReadOnlyList<LegacyTileSquareTile> tiles)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    int expectedTileCount = width * height;
    if (width == 0 || height == 0 || tiles.Count != expectedTileCount)
    {
      throw new ArgumentOutOfRangeException(nameof(tiles));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(x);
      writer.Write(y);
      writer.Write(width);
      writer.Write(height);
      writer.Write(changeType);
      for (int index = 0; index < tiles.Count; index++)
      {
        WriteTileSquareTile(writer, tiles[index]);
      }
    }

    return EncodeFrame(TerrariaMessageId.TileSquare, payload.ToArray());
  }

  public static byte[] EncodeNpcReplication(LegacyNpcWireState state)
  {
    NpcReplicationSnapshot snapshot = state.Snapshot;
    if (snapshot.ReplicationId is > short.MaxValue or < short.MinValue ||
        snapshot.NpcType is > short.MaxValue or < short.MinValue ||
        state.LifeMaximum < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(state));
    }

    int life = snapshot.IsActive ? snapshot.Health : 0;
    if (life < 0 || life > state.LifeMaximum)
    {
      throw new ArgumentOutOfRangeException(nameof(state));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.ReplicationId);
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Position.X));
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Position.Y));
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Velocity.X));
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Velocity.Y));
      writer.Write(state.Target);

      byte flags = 0;
      flags = SetBit(flags, 0, state.DirectionRight);
      flags = SetBit(flags, 1, state.DirectionYDown);
      flags = SetBit(flags, 2, state.Ai0 != 0);
      flags = SetBit(flags, 3, state.Ai1 != 0);
      flags = SetBit(flags, 4, state.Ai2 != 0);
      flags = SetBit(flags, 5, state.Ai3 != 0);
      flags = SetBit(flags, 6, state.SpriteDirectionRight);
      flags = SetBit(flags, 7, life == state.LifeMaximum);
      writer.Write(flags);

      byte flags2 = 0;
      flags2 = SetBit(flags2, 0, state.PlayersForScaling > 1);
      flags2 = SetBit(flags2, 1, state.SpawnedFromStatue);
      flags2 = SetBit(flags2, 2, state.Difficulty != 1);
      flags2 = SetBit(flags2, 3, state.SpawnNeedsSyncing);
      flags2 = SetBit(flags2, 4, state.SpawnNeedsSyncing && state.ShimmerTransparency > 0);
      writer.Write(flags2);

      WritePresentAi(writer, state.Ai0);
      WritePresentAi(writer, state.Ai1);
      WritePresentAi(writer, state.Ai2);
      WritePresentAi(writer, state.Ai3);
      writer.Write((short)snapshot.NpcType);
      if (state.PlayersForScaling > 1)
      {
        writer.Write(state.PlayersForScaling);
      }

      if (state.Difficulty != 1)
      {
        writer.Write(state.Difficulty);
      }

      if (life != state.LifeMaximum)
      {
        WriteNpcLife(writer, life, state.LifeMaximum);
      }

      if (state.IsCatchable)
      {
        writer.Write(state.ReleaseOwner);
      }
    }

    return EncodeFrame(TerrariaMessageId.SyncNPC, payload.ToArray());
  }

  private static byte[] EncodeFrame(TerrariaMessageId messageId, byte[] payload)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(messageId, payload));
  }

  private static SimulationVector ReadVector(BinaryReader reader)
  {
    return new SimulationVector(reader.ReadSingle(), reader.ReadSingle());
  }

  private static void RequireRemaining(MemoryStream stream, int length, string suffixName)
  {
    if (stream.Length - stream.Position < length)
    {
      throw new InvalidDataException(
        $"Terraria PlayerControls packet has a truncated {suffixName} suffix.");
    }
  }

  private static byte[] EncodeJoinWorldGreeting(string worldName)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      WriteNetTextModuleHeader(writer);
      WriteFormattableText(
        writer,
        "{0} {1}!",
        WriteLocalizationKeyText("LegacyMultiplayer.18"),
        WriteLiteralText(worldName));
      WriteJoinGreetingColor(writer);
    }

    return EncodeFrame(TerrariaMessageId.NetModules, payload.ToArray());
  }

  private static byte[] EncodeJoinPlayerGreeting(string playerName)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      WriteNetTextModuleHeader(writer);
      WriteLocalizationKeyText(writer, "Game.JoinGreeting", WriteLiteralText(playerName));
      WriteJoinGreetingColor(writer);
    }

    return EncodeFrame(TerrariaMessageId.NetModules, payload.ToArray());
  }

  private static void WriteNetTextModuleHeader(BinaryWriter writer)
  {
    const ushort netTextModuleId = 1;
    writer.Write(netTextModuleId);
    writer.Write(byte.MaxValue);
  }

  private static Action<BinaryWriter> WriteLiteralText(string value)
  {
    return writer =>
    {
      writer.Write((byte)0);
      writer.Write(value);
    };
  }

  private static Action<BinaryWriter> WriteLocalizationKeyText(string key)
  {
    return writer => WriteLocalizationKeyText(writer, key);
  }

  private static void WriteLocalizationKeyText(
    BinaryWriter writer,
    string key,
    params Action<BinaryWriter>[] substitutions)
  {
    writer.Write((byte)2);
    writer.Write(key);
    writer.Write((byte)substitutions.Length);
    for (int index = 0; index < substitutions.Length; index++)
    {
      substitutions[index].Invoke(writer);
    }
  }

  private static void WriteFormattableText(
    BinaryWriter writer,
    string format,
    params Action<BinaryWriter>[] substitutions)
  {
    writer.Write((byte)1);
    writer.Write(format);
    writer.Write((byte)substitutions.Length);
    for (int index = 0; index < substitutions.Length; index++)
    {
      substitutions[index].Invoke(writer);
    }
  }

  private static void WriteJoinGreetingColor(BinaryWriter writer)
  {
    writer.Write(byte.MaxValue);
    writer.Write((byte)240);
    writer.Write((byte)20);
  }

  private static byte SetBit(byte flags, int bit, bool value)
  {
    return value ? (byte)(flags | (1 << bit)) : flags;
  }

  private static void WriteWorldData(BinaryWriter writer, LegacyWorldDataContext context)
  {
    LegacyWorldBackgroundState background = context.Background;
    LegacyWorldProgressionState progression = context.Progression;
    LegacyOreTierState oreTiers = context.OreTiers;
    writer.Write(context.Time);
    writer.Write(context.WorldFlags);
    writer.Write(context.MoonPhase);
    writer.Write(context.Width);
    writer.Write(context.Height);
    writer.Write(context.SpawnX);
    writer.Write(context.SpawnY);
    writer.Write(context.WorldSurface);
    writer.Write(context.RockLayer);
    writer.Write(context.WorldId);
    writer.Write(context.WorldName);
    writer.Write(context.GameMode);
    writer.Write(context.UniqueId.ToByteArray());
    writer.Write(context.WorldGeneratorVersion);
    writer.Write(context.MoonType);
    writer.Write(background.TreeBackground1);
    writer.Write(background.TreeBackground2);
    writer.Write(background.TreeBackground3);
    writer.Write(background.TreeBackground4);
    writer.Write(background.CorruptionBackground);
    writer.Write(background.JungleBackground);
    writer.Write(background.SnowBackground);
    writer.Write(background.HallowBackground);
    writer.Write(background.CrimsonBackground);
    writer.Write(background.DesertBackground);
    writer.Write(background.OceanBackground);
    writer.Write(background.MushroomBackground);
    writer.Write(background.UnderworldBackground);
    writer.Write(background.IceBackgroundStyle);
    writer.Write(background.JungleBackgroundStyle);
    writer.Write(background.HellBackgroundStyle);
    writer.Write(background.WindSpeedTarget);
    writer.Write(background.CloudCount);
    writer.Write(background.TreeX1);
    writer.Write(background.TreeX2);
    writer.Write(background.TreeX3);
    writer.Write(background.TreeStyle1);
    writer.Write(background.TreeStyle2);
    writer.Write(background.TreeStyle3);
    writer.Write(background.TreeStyle4);
    writer.Write(background.CaveBackgroundX1);
    writer.Write(background.CaveBackgroundX2);
    writer.Write(background.CaveBackgroundX3);
    writer.Write(background.CaveBackgroundStyle1);
    writer.Write(background.CaveBackgroundStyle2);
    writer.Write(background.CaveBackgroundStyle3);
    writer.Write(background.CaveBackgroundStyle4);
    writer.Write(background.TreeTopStyle1);
    writer.Write(background.TreeTopStyle2);
    writer.Write(background.TreeTopStyle3);
    writer.Write(background.TreeTopStyle4);
    writer.Write(background.TreeTopStyle5);
    writer.Write(background.TreeTopStyle6);
    writer.Write(background.TreeTopStyle7);
    writer.Write(background.TreeTopStyle8);
    writer.Write(background.TreeTopStyle9);
    writer.Write(background.TreeTopStyle10);
    writer.Write(background.TreeTopStyle11);
    writer.Write(background.TreeTopStyle12);
    writer.Write(background.TreeTopStyle13);
    writer.Write(background.MaximumRaining);
    writer.Write(progression.EventFlags1);
    writer.Write(progression.EventFlags2);
    writer.Write(progression.EventFlags3);
    writer.Write(progression.EventFlags4);
    writer.Write(progression.EventFlags5);
    writer.Write(progression.EventFlags6);
    writer.Write(progression.EventFlags7);
    writer.Write(progression.EventFlags8);
    writer.Write(progression.EventFlags9);
    writer.Write(progression.EventFlags10);
    writer.Write(progression.EventFlags11);
    byte worldVariantFlags = context.IsNoTrapsWorld ? (byte)1 : (byte)0;
    if (context.IsSkyblockWorld)
    {
      worldVariantFlags |= 1 << 6;
    }

    writer.Write(worldVariantFlags);
    writer.Write(progression.SundialCooldown);
    writer.Write(progression.MoondialCooldown);
    writer.Write(oreTiers.Copper);
    writer.Write(oreTiers.Iron);
    writer.Write(oreTiers.Silver);
    writer.Write(oreTiers.Gold);
    writer.Write(oreTiers.Cobalt);
    writer.Write(oreTiers.Mythril);
    writer.Write(oreTiers.Adamantite);
    writer.Write(progression.InvasionType);
    writer.Write(progression.LobbyId);
    writer.Write(progression.IntendedSandstormSeverity);
    writer.Write((byte)context.ExtraSpawnPoints.Count);
    for (int index = 0; index < context.ExtraSpawnPoints.Count; index++)
    {
      LegacySpawnPoint point = context.ExtraSpawnPoints[index];
      writer.Write(point.X);
      writer.Write(point.Y);
    }
  }

  private static void WriteTileSectionTiles(BinaryWriter writer, WorldSectionSnapshot snapshot)
  {
    int tileCount = snapshot.Width * snapshot.Height;
    List<LegacyTileSectionTile> tiles = new(tileCount);
    for (int index = 0; index < tileCount; index++)
    {
      tiles.Add(CreateLegacyTileSectionTile(GetTileByIndex(snapshot, index)));
    }

    WriteTileSectionTiles(writer, tiles);
  }

  private static LegacyTileSectionTile CreateLegacyTileSectionTile(WorldTile tile)
  {
    bool isFrameImportant = tile.IsActive && FrameImportantTileTypes.Contains(tile.Type);
    return new LegacyTileSectionTile(
      tile.IsActive,
      tile.WallType != 0,
      tile.LiquidAmount,
      GetLegacyLiquidKind(tile),
      tile.HasWire,
      tile.HasWire2,
      tile.HasWire3,
      tile.HasWire4,
      tile.IsHalfBrick,
      tile.Slope,
      tile.IsActuated,
      tile.IsInactive,
      tile.TileColor,
      tile.WallColor,
      tile.IsInvisibleBlock,
      tile.IsInvisibleWall,
      tile.IsFullbrightBlock,
      tile.IsFullbrightWall,
      tile.Type,
      isFrameImportant,
      tile.FrameX,
      tile.FrameY,
      tile.WallType,
      !isFrameImportant);
  }

  private static LegacyTileSectionLiquidKind GetLegacyLiquidKind(WorldTile tile)
  {
    if (tile.LiquidAmount == 0)
    {
      return LegacyTileSectionLiquidKind.Water;
    }

    return tile.LiquidType switch
    {
      0 => LegacyTileSectionLiquidKind.Water,
      1 => LegacyTileSectionLiquidKind.Lava,
      2 => LegacyTileSectionLiquidKind.Honey,
      3 => LegacyTileSectionLiquidKind.Shimmer,
      _ => throw new ArgumentOutOfRangeException(nameof(tile))
    };
  }

  private static void WriteTileSectionTiles(
    BinaryWriter writer,
    IReadOnlyList<LegacyTileSectionTile> tiles)
  {
    int index = 0;
    while (index < tiles.Count)
    {
      LegacyTileSectionTile tile = tiles[index];
      int repeatCount = 0;
      while (index + repeatCount + 1 < tiles.Count &&
             tiles[index + repeatCount + 1] == tile &&
             tiles[index + repeatCount + 1].AllowsRleBatching &&
             repeatCount < short.MaxValue)
      {
        repeatCount++;
      }

      WriteTileSectionTile(writer, tile, repeatCount);
      index += repeatCount + 1;
    }
  }

  private static void WriteTileSectionTile(
    BinaryWriter writer,
    LegacyTileSectionTile tile,
    int repeatCount)
  {
    ValidateTileSectionTile(tile);

    byte primaryHeader = 0;
    byte secondaryHeader = 0;
    byte tertiaryHeader = 0;
    byte quaternaryHeader = 0;
    if (tile.IsActive)
    {
      primaryHeader |= 2;
      if (tile.TileType > byte.MaxValue)
      {
        primaryHeader |= 0x20;
      }
    }

    if (tile.HasWall)
    {
      primaryHeader |= 4;
      if (tile.WallType > byte.MaxValue)
      {
        tertiaryHeader |= 0x40;
      }
    }

    if (tile.LiquidAmount != 0)
    {
      switch (tile.LiquidKind)
      {
        case LegacyTileSectionLiquidKind.Water:
          primaryHeader |= 8;
          break;
        case LegacyTileSectionLiquidKind.Lava:
          primaryHeader |= 0x10;
          break;
        case LegacyTileSectionLiquidKind.Honey:
          primaryHeader |= 0x18;
          break;
        case LegacyTileSectionLiquidKind.Shimmer:
          primaryHeader |= 8;
          tertiaryHeader |= 0x80;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(tile));
      }
    }

    if (tile.HasWire1)
    {
      secondaryHeader |= 2;
    }

    if (tile.HasWire2)
    {
      secondaryHeader |= 4;
    }

    if (tile.HasWire3)
    {
      secondaryHeader |= 8;
    }

    if (tile.IsHalfBrick)
    {
      secondaryHeader |= 0x10;
    }
    else if (tile.Slope != 0)
    {
      secondaryHeader |= (byte)((tile.Slope + 1) << 4);
    }

    if (tile.HasActuator)
    {
      tertiaryHeader |= 2;
    }

    if (tile.IsInactive)
    {
      tertiaryHeader |= 4;
    }

    if (tile.TileColor != 0)
    {
      tertiaryHeader |= 8;
    }

    if (tile.WallColor != 0)
    {
      tertiaryHeader |= 0x10;
    }

    if (tile.HasWire4)
    {
      tertiaryHeader |= 0x20;
    }

    if (tile.IsInvisibleBlock)
    {
      quaternaryHeader |= 2;
    }

    if (tile.IsInvisibleWall)
    {
      quaternaryHeader |= 4;
    }

    if (tile.IsFullBrightBlock)
    {
      quaternaryHeader |= 8;
    }

    if (tile.IsFullBrightWall)
    {
      quaternaryHeader |= 0x10;
    }

    if (quaternaryHeader != 0)
    {
      tertiaryHeader |= 1;
    }

    if (tertiaryHeader != 0)
    {
      secondaryHeader |= 1;
    }

    if (secondaryHeader != 0)
    {
      primaryHeader |= 1;
    }

    if (repeatCount > byte.MaxValue)
    {
      primaryHeader |= 0x80;
    }
    else if (repeatCount > 0)
    {
      primaryHeader |= 0x40;
    }

    writer.Write(primaryHeader);
    if (secondaryHeader != 0)
    {
      writer.Write(secondaryHeader);
    }

    if (tertiaryHeader != 0)
    {
      writer.Write(tertiaryHeader);
    }

    if (quaternaryHeader != 0)
    {
      writer.Write(quaternaryHeader);
    }

    if (tile.IsActive)
    {
      writer.Write((byte)tile.TileType);
      if (tile.TileType > byte.MaxValue)
      {
        writer.Write((byte)(tile.TileType >> 8));
      }

      if (tile.IsFrameImportant)
      {
        writer.Write(tile.FrameX);
        writer.Write(tile.FrameY);
      }

      if (tile.TileColor != 0)
      {
        writer.Write(tile.TileColor);
      }
    }

    if (tile.HasWall)
    {
      writer.Write((byte)tile.WallType);
      if (tile.WallColor != 0)
      {
        writer.Write(tile.WallColor);
      }
    }

    if (tile.LiquidAmount != 0)
    {
      writer.Write(tile.LiquidAmount);
    }

    if (tile.HasWall && tile.WallType > byte.MaxValue)
    {
      writer.Write((byte)(tile.WallType >> 8));
    }

    if (repeatCount > byte.MaxValue)
    {
      writer.Write((short)repeatCount);
    }
    else if (repeatCount > 0)
    {
      writer.Write((byte)repeatCount);
    }
  }

  private static void ValidateTileSectionTile(LegacyTileSectionTile tile)
  {
    if (tile.HasWall != (tile.WallType != 0) ||
        tile.IsHalfBrick && tile.Slope != 0 ||
        tile.Slope > 4 ||
        !tile.IsActive && (tile.IsFrameImportant || tile.TileColor != 0) ||
        !tile.HasWall && tile.WallColor != 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tile));
    }
  }

  private static void WriteTileEntities(
    BinaryWriter writer,
    IReadOnlyList<LegacyTileEntity> tileEntities)
  {
    if (tileEntities.Count > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tileEntities));
    }

    writer.Write((short)tileEntities.Count);
    for (int index = 0; index < tileEntities.Count; index++)
    {
      WriteTileEntity(writer, tileEntities[index]);
    }
  }

  private static void WriteTileEntity(BinaryWriter writer, LegacyTileEntity tileEntity)
  {
    ArgumentNullException.ThrowIfNull(tileEntity);
    switch (tileEntity)
    {
      case LegacyTrainingDummyTileEntity trainingDummy:
        WriteTileEntityHeader(writer, 0, trainingDummy);
        writer.Write(trainingDummy.NpcId);
        return;
      case LegacyItemTileEntity itemEntity:
        WriteItemTileEntity(writer, itemEntity);
        return;
      case LegacyHatRackTileEntity hatRack:
        WriteHatRackTileEntity(writer, hatRack);
        return;
      case LegacyDisplayDollTileEntity displayDoll:
        WriteDisplayDollTileEntity(writer, displayDoll);
        return;
      case LegacyTeleportationPylonTileEntity pylon:
        WriteTileEntityHeader(writer, 7, pylon);
        return;
      default:
        throw new ArgumentOutOfRangeException(nameof(tileEntity));
    }
  }

  private static void WriteItemTileEntity(BinaryWriter writer, LegacyItemTileEntity tileEntity)
  {
    if (tileEntity.Kind is not LegacyTileEntityItemKind.ItemFrame and
        not LegacyTileEntityItemKind.WeaponsRack and
        not LegacyTileEntityItemKind.FoodPlatter and
        not LegacyTileEntityItemKind.DeadCellsDisplayJar)
    {
      throw new ArgumentOutOfRangeException(nameof(tileEntity));
    }

    WriteTileEntityHeader(writer, (byte)tileEntity.Kind, tileEntity);
    WriteTileEntityItem(writer, tileEntity.Item);
  }

  private static void WriteHatRackTileEntity(BinaryWriter writer, LegacyHatRackTileEntity tileEntity)
  {
    WriteTileEntityHeader(writer, 5, tileEntity);
    LegacyTileEntityItem?[] items = [
      tileEntity.Item0,
      tileEntity.Item1,
      tileEntity.Dye0,
      tileEntity.Dye1
    ];
    byte mask = GetTileEntityItemMask(items);
    writer.Write(mask);
    WritePresentTileEntityItems(writer, items);
  }

  private static void WriteDisplayDollTileEntity(
    BinaryWriter writer,
    LegacyDisplayDollTileEntity tileEntity)
  {
    ArgumentNullException.ThrowIfNull(tileEntity.Equipment);
    ArgumentNullException.ThrowIfNull(tileEntity.Dyes);
    if (tileEntity.Equipment.Count != 9 || tileEntity.Dyes.Count != 9)
    {
      throw new ArgumentOutOfRangeException(nameof(tileEntity));
    }

    WriteTileEntityHeader(writer, 3, tileEntity);
    byte equipmentMask = GetTileEntityItemMask(tileEntity.Equipment, 8);
    byte dyeMask = GetTileEntityItemMask(tileEntity.Dyes, 8);
    byte extraMask = GetTileEntityItemMask([
      tileEntity.Misc,
      tileEntity.Equipment[8],
      tileEntity.Dyes[8]
    ], 3);
    writer.Write(equipmentMask);
    writer.Write(dyeMask);
    writer.Write(tileEntity.Pose);
    writer.Write(extraMask);
    WritePresentTileEntityItems(writer, tileEntity.Equipment);
    WritePresentTileEntityItems(writer, tileEntity.Dyes);
    if (tileEntity.Misc is LegacyTileEntityItem misc)
    {
      WriteTileEntityItem(writer, misc);
    }
  }

  private static void WriteTileEntityHeader(
    BinaryWriter writer,
    byte type,
    LegacyTileEntity tileEntity)
  {
    writer.Write(type);
    writer.Write(tileEntity.EntityId);
    writer.Write(tileEntity.TileX);
    writer.Write(tileEntity.TileY);
  }

  private static byte GetTileEntityItemMask(
    IReadOnlyList<LegacyTileEntityItem?> items,
    int count = 4)
  {
    byte mask = 0;
    for (int index = 0; index < count; index++)
    {
      if (items[index].HasValue)
      {
        mask |= (byte)(1 << index);
      }
    }

    return mask;
  }

  private static void WritePresentTileEntityItems(
    BinaryWriter writer,
    IReadOnlyList<LegacyTileEntityItem?> items)
  {
    for (int index = 0; index < items.Count; index++)
    {
      if (items[index] is LegacyTileEntityItem item)
      {
        WriteTileEntityItem(writer, item);
      }
    }
  }

  private static void WriteTileEntityItem(BinaryWriter writer, LegacyTileEntityItem item)
  {
    if (item.ItemType > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(item));
    }

    writer.Write((short)item.ItemType);
    writer.Write(item.Prefix);
    writer.Write(item.Stack);
  }

  private static WorldTile GetTileByIndex(WorldSectionSnapshot snapshot, int index)
  {
    int x = index % snapshot.Width;
    int y = index / snapshot.Width;
    return snapshot.GetTile(x, y);
  }

  private static void WriteSectionChests(
    BinaryWriter writer,
    WorldSectionCoordinates section,
    IReadOnlyList<ChestSnapshot> chests)
  {
    int chestCount = 0;
    for (int index = 0; index < chests.Count; index++)
    {
      if (chests[index].Section == section)
      {
        chestCount++;
      }
    }

    writer.Write((short)chestCount);
    for (int index = 0; index < chests.Count; index++)
    {
      ChestSnapshot chest = chests[index];
      if (chest.Section != section)
      {
        continue;
      }

      if (chest.ChestId > short.MaxValue || chest.TileX > short.MaxValue ||
          chest.TileY > short.MaxValue)
      {
        throw new ArgumentOutOfRangeException(nameof(chests));
      }

      writer.Write((short)chest.ChestId);
      writer.Write((short)chest.TileX);
      writer.Write((short)chest.TileY);
      writer.Write(string.Empty);
    }
  }

  private static void WriteSectionSigns(
    BinaryWriter writer,
    WorldSectionCoordinates section,
    IReadOnlyList<SignReplicationSnapshot> signs)
  {
    int signCount = 0;
    for (int index = 0; index < signs.Count; index++)
    {
      if (IsSignInSection(signs[index], section))
      {
        signCount++;
      }
    }

    if (signCount > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(signs));
    }

    writer.Write((short)signCount);
    for (int index = 0; index < signs.Count; index++)
    {
      SignReplicationSnapshot sign = signs[index];
      if (!IsSignInSection(sign, section))
      {
        continue;
      }

      if (sign.SignId < short.MinValue || sign.SignId > short.MaxValue)
      {
        throw new ArgumentOutOfRangeException(nameof(signs));
      }

      ArgumentNullException.ThrowIfNull(sign.Text);
      writer.Write((short)sign.SignId);
      writer.Write(sign.TileX);
      writer.Write(sign.TileY);
      writer.Write(sign.Text);
    }
  }

  private static bool IsSignInSection(
    SignReplicationSnapshot sign,
    WorldSectionCoordinates section)
  {
    if (sign.TileX < 0 || sign.TileY < 0)
    {
      return false;
    }

    WorldSectionCoordinates signSection = new(
      sign.TileX / WorldGrid.SectionWidth,
      sign.TileY / WorldGrid.SectionHeight);
    return signSection == section;
  }

  private static void WriteTileSquareTile(BinaryWriter writer, WorldTile tile)
  {
    byte flags = tile.IsActive ? (byte)1 : (byte)0;
    if (tile.LiquidAmount > 0)
    {
      flags |= 1 << 3;
    }

    writer.Write(flags);
    writer.Write((byte)0);
    writer.Write((byte)0);
    if (tile.IsActive)
    {
      writer.Write(tile.Type);
    }

    if (tile.LiquidAmount > 0)
    {
      writer.Write(tile.LiquidAmount);
      writer.Write(tile.LiquidType);
    }
  }

  private static void WriteTileSquareTile(BinaryWriter writer, LegacyTileSquareTile tile)
  {
    if (tile.Slope > 7 || tile.HasWall != (tile.WallType != 0) ||
        tile.HasLiquid != (tile.LiquidAmount != 0))
    {
      throw new ArgumentOutOfRangeException(nameof(tile));
    }

    byte flags = 0;
    flags = SetBit(flags, 0, tile.IsActive);
    flags = SetBit(flags, 2, tile.HasWall);
    flags = SetBit(flags, 3, tile.HasLiquid);
    flags = SetBit(flags, 4, tile.HasWire);
    flags = SetBit(flags, 5, tile.IsHalfBrick);
    flags = SetBit(flags, 6, tile.HasActuator);
    flags = SetBit(flags, 7, tile.IsInactive);

    byte flags2 = 0;
    flags2 = SetBit(flags2, 0, tile.HasWire2);
    flags2 = SetBit(flags2, 1, tile.HasWire3);
    flags2 = SetBit(flags2, 2, tile.TileColor != 0);
    flags2 = SetBit(flags2, 3, tile.WallColor != 0);
    flags2 |= (byte)(tile.Slope << 4);
    flags2 = SetBit(flags2, 7, tile.HasWire4);

    byte flags3 = 0;
    flags3 = SetBit(flags3, 0, tile.IsFullBrightBlock);
    flags3 = SetBit(flags3, 1, tile.IsFullBrightWall);
    flags3 = SetBit(flags3, 2, tile.IsInvisibleBlock);
    flags3 = SetBit(flags3, 3, tile.IsInvisibleWall);

    writer.Write(flags);
    writer.Write(flags2);
    writer.Write(flags3);
    if (tile.TileColor != 0)
    {
      writer.Write(tile.TileColor);
    }

    if (tile.WallColor != 0)
    {
      writer.Write(tile.WallColor);
    }

    if (tile.IsActive)
    {
      writer.Write(tile.TileType);
      if (tile.IsFrameImportant)
      {
        writer.Write(tile.FrameX);
        writer.Write(tile.FrameY);
      }
    }

    if (tile.HasWall)
    {
      writer.Write(tile.WallType);
    }

    if (tile.HasLiquid)
    {
      writer.Write(tile.LiquidAmount);
      writer.Write(tile.LiquidType);
    }
  }

  private static void WritePresentAi(BinaryWriter writer, float value)
  {
    if (value != 0)
    {
      writer.Write(value);
    }
  }

  private static void WriteNpcLife(BinaryWriter writer, int life, int lifeMaximum)
  {
    if (lifeMaximum > short.MaxValue)
    {
      writer.Write((byte)4);
      writer.Write(life);
      return;
    }

    if (lifeMaximum > sbyte.MaxValue)
    {
      writer.Write((byte)2);
      writer.Write((short)life);
      return;
    }

    writer.Write((byte)1);
    writer.Write((sbyte)life);
  }
}
