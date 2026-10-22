using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects captured world sections into bounded Version4 section packets.</summary>
public sealed class WorldSectionPacketEncoder
{
  private const int MaximumSectionPacketBodyBytes = PacketFrameCodec.MaximumPayloadBytes;
  private readonly WorldFacts _facts;
  private readonly Version4WireProfile _wireProfile;

  public WorldSectionPacketEncoder(WorldFacts facts, Version4WireProfile wireProfile)
  {
    ArgumentNullException.ThrowIfNull(facts);
    ArgumentNullException.ThrowIfNull(wireProfile);
    _facts = facts;
    _wireProfile = wireProfile;
  }

  /// <summary>
  /// Encodes a section snapshot in one or more legal packet-10 rectangles. Attachments are
  /// supplied by their owning domains and included once, at their anchor coordinate.
  /// </summary>
  public IReadOnlyList<Version4EncodedPacket> EncodeSection(
    WorldSectionSnapshot section,
    WorldSectionPacketAttachments attachments)
  {
    return EncodeSection(section, attachments, out _);
  }

  /// <summary>Retains the typed fragments for a prepared cross-owner projection.</summary>
  internal IReadOnlyList<Version4EncodedPacket> EncodeSection(
    WorldSectionSnapshot section,
    WorldSectionPacketAttachments attachments,
    out IReadOnlyList<TileSectionPacket> typedPackets)
  {
    ArgumentNullException.ThrowIfNull(section);
    ArgumentNullException.ThrowIfNull(attachments);
    ValidateSection(section);
    if (attachments.Version != section.Version)
    {
      throw new InvalidOperationException(
        "Section attachments must be captured at the section's world commit version.");
    }

    ValidateAttachments(section.Bounds, attachments);

    var packets = new List<Version4EncodedPacket>();
    var typed = new List<TileSectionPacket>();
    EncodeRectangle(
      section,
      attachments,
      new WorldSectionBounds(
        section.Bounds.OriginX,
        section.Bounds.OriginY,
        section.Bounds.Width,
        section.Bounds.Height),
      packets,
      typed);
    typedPackets = Array.AsReadOnly(typed.ToArray());
    return Array.AsReadOnly(packets.ToArray());
  }

  /// <summary>Encodes the Version4 packet-11 frame range for a representable section.</summary>
  public Version4EncodedPacket EncodeTileFrameSection(WorldSectionBounds bounds)
  {
    if (bounds.OriginX < 0 || bounds.OriginY < 0 ||
        bounds.OriginX > short.MaxValue || bounds.OriginY > short.MaxValue ||
        bounds.Width <= 0 || bounds.Width > short.MaxValue ||
        bounds.Height <= 0 || bounds.Height > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(bounds));
    }

    var packet = new TileFrameSectionPacket
    {
      X = (short)bounds.OriginX,
      Y = (short)bounds.OriginY,
      Width = (short)bounds.Width,
      Height = (short)bounds.Height
    };
    return _wireProfile.Encode(WireDirection.ServerToClient, packet);
  }

  private static bool IsFrameLimitFailure(InvalidOperationException exception)
  {
    return exception.Message.Contains(
      "was rejected or exceeds the frame body limit",
      StringComparison.Ordinal);
  }

  private void EncodeRectangle(
    WorldSectionSnapshot section,
    WorldSectionPacketAttachments attachments,
    WorldSectionBounds rectangle,
    List<Version4EncodedPacket> packets,
    List<TileSectionPacket> typedPackets)
  {
    TileSectionPacket packet = CreatePacket(section, attachments, rectangle);
    try
    {
      Version4EncodedPacket encoded = _wireProfile.Encode(WireDirection.ServerToClient, packet);
      if (encoded.Payload.Length > MaximumSectionPacketBodyBytes)
      {
        throw new InvalidOperationException("The encoded packet-10 body exceeds the frame limit.");
      }

      packets.Add(encoded);
      typedPackets.Add(packet);
    }
    catch (InvalidOperationException exception) when (IsFrameLimitFailure(exception))
    {
      if (rectangle.Width == 1 && rectangle.Height == 1)
      {
        throw;
      }

      SplitRectangle(section, attachments, rectangle, packets, typedPackets);
    }
  }

  private void SplitRectangle(
    WorldSectionSnapshot section,
    WorldSectionPacketAttachments attachments,
    WorldSectionBounds rectangle,
    List<Version4EncodedPacket> packets,
    List<TileSectionPacket> typedPackets)
  {
    if (rectangle.Height >= rectangle.Width && rectangle.Height > 1)
    {
      int firstHeight = rectangle.Height / 2;
      EncodeRectangle(
        section,
        attachments,
        rectangle with { Height = firstHeight },
        packets,
        typedPackets);
      EncodeRectangle(
        section,
        attachments,
        rectangle with
        {
          OriginY = rectangle.OriginY + firstHeight,
          Height = rectangle.Height - firstHeight
        },
        packets,
        typedPackets);
      return;
    }

    int firstWidth = rectangle.Width / 2;
    EncodeRectangle(
      section,
      attachments,
      rectangle with { Width = firstWidth },
      packets,
      typedPackets);
    EncodeRectangle(
      section,
      attachments,
      rectangle with
      {
        OriginX = rectangle.OriginX + firstWidth,
        Width = rectangle.Width - firstWidth
      },
      packets,
      typedPackets);
  }

  private TileSectionPacket CreatePacket(
    WorldSectionSnapshot section,
    WorldSectionPacketAttachments attachments,
    WorldSectionBounds rectangle)
  {
    int localX = rectangle.OriginX - section.Bounds.OriginX;
    int localY = rectangle.OriginY - section.Bounds.OriginY;
    var tiles = new Packet10Tile[checked(rectangle.Width * rectangle.Height)];
    int index = 0;
    for (int y = 0; y < rectangle.Height; y++)
    {
      for (int x = 0; x < rectangle.Width; x++)
      {
        tiles[index++] = ProjectTile(section.GetTile(localX + x, localY + y));
      }
    }

    Packet10ChestRecord[] chests = attachments.Chests
      .Where(chest => rectangle.Contains(new TileCoordinate(chest.X, chest.Y)))
      .ToArray();
    Packet10SignRecord[] signs = attachments.Signs
      .Where(sign => rectangle.Contains(new TileCoordinate(sign.X, sign.Y)))
      .ToArray();
    PacketTileEntityRecord[] tileEntities = attachments.TileEntities
      .Where(entity => rectangle.Contains(new TileCoordinate(entity.X, entity.Y)))
      .ToArray();

    return new TileSectionPacket
    {
      StartX = rectangle.OriginX,
      StartY = rectangle.OriginY,
      Width = checked((short)rectangle.Width),
      Height = checked((short)rectangle.Height),
      Tiles = tiles,
      Chests = chests,
      Signs = signs,
      TileEntities = tileEntities
    };
  }

  private Packet10Tile ProjectTile(WorldTile tile)
  {
    if (!_facts.IsValidTileType(tile.Type) || !Enum.IsDefined(tile.LiquidType) ||
        tile.TileColor > 31 || tile.WallColor > 31 || tile.Slope > 4 ||
        (tile.HalfBlock && tile.Slope != 0) ||
        (tile.TileHeader3 & 0x1f) != 0 || (tile.WallHeader & 0x7f) != 0)
    {
      throw new InvalidOperationException(
        "A world tile contains values outside the Version4 packet-10 facts.");
    }

    return new Packet10Tile
    {
      Active = tile.Active,
      Type = tile.Type,
      FrameX = tile.FrameX,
      FrameY = tile.FrameY,
      TileColor = tile.TileColor,
      Wall = tile.Wall,
      WallColor = tile.WallColor,
      LiquidAmount = tile.LiquidAmount,
      LiquidType = (Packet10LiquidType)tile.LiquidType,
      Wire = (tile.Wires & TileWireMask.Red) != 0,
      Wire2 = (tile.Wires & TileWireMask.Blue) != 0,
      Wire3 = (tile.Wires & TileWireMask.Green) != 0,
      Wire4 = (tile.Wires & TileWireMask.Yellow) != 0,
      HalfBrick = tile.HalfBlock,
      Slope = tile.Slope,
      Actuator = tile.Actuator,
      Inactive = tile.Inactive,
      InvisibleBlock = (tile.TileHeader3 & 0x20) != 0,
      InvisibleWall = (tile.TileHeader3 & 0x40) != 0,
      FullbrightBlock = (tile.TileHeader3 & 0x80) != 0,
      FullbrightWall = (tile.WallHeader & 0x80) != 0
    };
  }

  private static void ValidateAttachments(
    WorldSectionBounds sectionBounds,
    WorldSectionPacketAttachments attachments)
  {
    var chestIds = new HashSet<short>();
    foreach (Packet10ChestRecord chest in attachments.Chests)
    {
      if (chest.Id < 0 || chest.X < 0 || chest.Y < 0 || chest.Name is null ||
          !sectionBounds.Contains(new TileCoordinate(chest.X, chest.Y)) ||
          !chestIds.Add(chest.Id))
      {
        throw new ArgumentException(
          "Chest attachments must be unique, named and anchored inside the section.",
          nameof(attachments));
      }
    }

    var signIds = new HashSet<short>();
    foreach (Packet10SignRecord sign in attachments.Signs)
    {
      if (sign.Id < 0 || sign.X < 0 || sign.Y < 0 || sign.Text is null ||
          !sectionBounds.Contains(new TileCoordinate(sign.X, sign.Y)) ||
          !signIds.Add(sign.Id))
      {
        throw new ArgumentException(
          "Sign attachments must be unique, named and anchored inside the section.",
          nameof(attachments));
      }
    }

    var tileEntityIds = new HashSet<int>();
    foreach (PacketTileEntityRecord entity in attachments.TileEntities)
    {
      if (entity.Id is not int id || id < 0 || entity.X < 0 || entity.Y < 0 ||
          !sectionBounds.Contains(new TileCoordinate(entity.X, entity.Y)) ||
          !tileEntityIds.Add(id))
      {
        throw new ArgumentException(
          "Tile entity attachments must have unique IDs and anchors inside the section.",
          nameof(attachments));
      }
    }
  }

  private static void ValidateSection(WorldSectionSnapshot section)
  {
    if (section.Bounds.Width <= 0 || section.Bounds.Width > short.MaxValue ||
        section.Bounds.Height <= 0 || section.Bounds.Height > short.MaxValue ||
        section.Tiles.Count != checked(section.Bounds.Width * section.Bounds.Height))
    {
      throw new ArgumentException(
        "The section snapshot dimensions or tile count are invalid.",
        nameof(section));
    }
  }
}
