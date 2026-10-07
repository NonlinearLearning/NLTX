using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text.Json;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class LiveTilePlacementVerification {
  public static int Run(int port, string factsPath, string outputPath) {
    var result = new Dictionary<string, object> {
      ["port"] = port, ["helloVersion"] = "Terraria326", ["passed"] = false
    };
    int exitCode = 1;
    try {
      using JsonDocument facts = JsonDocument.Parse(File.ReadAllText(factsPath));
      bool[] framed = facts.RootElement.GetProperty("frameImportant").EnumerateArray()
          .Select(value => value.GetBoolean()).ToArray();
      var inputs = new ProtocolInputs(framed, Enumerable.Repeat(true, framed.Length).ToArray(),
          PacketTileEntityCodecsV4.Create(), true, new bool[65536],
          static (_, _, _) => 4, static _ => false, 40,
          Packet82KnownModuleCodecsV4.CreateSteam(), 200, static _ => false);
      ProtocolProfile profile = SteamProtocolProfile.Create(new ProtocolFacts(inputs));
      using var client = new TcpClient { ReceiveTimeout = 5000, SendTimeout = 5000 };
      client.Connect("127.0.0.1", port);
      using NetworkStream stream = client.GetStream();
      Send(stream, profile, new HelloPacket { Version = "Terraria326" });
      byte player = ((PlayerInfoPacket)Read(stream, profile, 3)).Player;
      Send(stream, profile, new RequestWorldDataPacket());
      var world = (WorldDataPacket)Read(stream, profile, 7);
      Send(stream, profile, new SpawnTileDataPacket { X = -1, Y = -1 });
      var sections = new List<TileSectionPacket>();
      while (true) {
        (byte id, object packet) = ReadFrame(stream, profile);
        if (id == 49) {
          break;
        }
        Verify.That(id is 9 or 10, "Initial synchronization returned an unexpected message.");
        if (packet is TileSectionPacket section) {
          sections.Add(section);
        }
      }
      TileSectionPacket target = sections.Single(section =>
          world.SpawnTileX >= section.StartX && world.SpawnTileX < section.StartX + section.Width
          && world.SpawnTileY >= section.StartY
          && world.SpawnTileY < section.StartY + section.Height);
      var empty = new List<(short X, short Y)>();
      for (int y = world.SpawnTileY - 4; y <= world.SpawnTileY + 4; y++) {
        for (int x = world.SpawnTileX - 4; x <= world.SpawnTileX + 4; x++) {
          if (x >= target.StartX && x < target.StartX + target.Width
              && y >= target.StartY && y < target.StartY + target.Height
              && !TileAt(target, (short)x, (short)y).Active) {
            empty.Add(((short)x, (short)y));
          }
        }
      }
      const int placements = 12;
      Verify.That(empty.Count > placements, "The world needs thirteen empty cells near spawn.");
      Send(stream, profile, new PlayerSpawnPacket { Player = player, SpawnX = -1, SpawnY = -1 });
      _ = Read(stream, profile, 129);
      Send(stream, profile, new PlayerControlsPacket {
        Player = player,
        Position = new PacketVector2(world.SpawnTileX * 16 + 8, world.SpawnTileY * 16 + 8)
      });
      for (int index = 0; index < placements; index++) {
        (short x, short y) = empty[index];
        short type = (short)(index % 2);
        Send(stream, profile, new TileManipulationPacket {
          Action = 1, X = x, Y = y, TileOrWallType = type
        });
        var placed = (AreaTileChangePacket)Read(stream, profile, 20);
        Verify.That(placed.StartX == x && placed.StartY == y && placed.Tiles[0].Active
            && placed.Tiles[0].TileType == type,
            "The placement reply must contain the committed block and its coordinates.");
        Send(stream, profile, new TileManipulationPacket {
          Action = 1, X = x, Y = y, TileOrWallType = (short)(1 - type)
        });
        var repeated = (AreaTileChangePacket)Read(stream, profile, 20);
        Verify.That(repeated.Tiles[0].Active && repeated.Tiles[0].TileType == type,
            "Repeated placement must preserve the occupied cell without disconnecting.");
      }
      Send(stream, profile, new RequestSectionPacket {
        SectionX = (ushort)(world.SpawnTileX / 200), SectionY = (ushort)(world.SpawnTileY / 150)
      });
      _ = Read(stream, profile, 9);
      var refreshed = (TileSectionPacket)Read(stream, profile, 10);
      for (int index = 0; index < placements; index++) {
        (short x, short y) = empty[index];
        Packet10Tile tile = TileAt(refreshed, x, y);
        Verify.That(tile.Active && tile.Type == index % 2,
            "The requested section must contain every placement after cache invalidation.");
      }
      (short framedX, short framedY) = empty[placements];
      int framedType = Array.FindIndex(framed, value => value);
      Verify.That(framedType >= 0, "The fixture must include a framed tile type.");
      Send(stream, profile, new TileManipulationPacket {
        Action = 1, X = framedX, Y = framedY, TileOrWallType = checked((short)framedType)
      });
      var correction = (AreaTileChangePacket)Read(stream, profile, 20);
      Verify.That(!correction.Tiles[0].Active,
          "An unsupported framed object must be corrected without closing the connection.");
      const int pingReplies = 20;
      for (int index = 0; index < pingReplies; index++) {
        Thread.Sleep(250);
        Send(stream, profile, new PingPacket());
        _ = Read(stream, profile, 154);
      }
      result["initialSections"] = sections.Count;
      result["placementsConfirmed"] = placements;
      result["occupiedCellCorrections"] = placements;
      result["sectionContainsPlacements"] = true;
      result["framedObjectCorrectedWithoutDisconnect"] = true;
      result["latencyRepliesAfterPlacement"] = pingReplies;
      result["cells"] = empty.Take(placements).Select(cell => new { cell.X, cell.Y }).ToArray();
      result["passed"] = true;
      exitCode = 0;
    } catch (Exception exception) {
      result["error"] = exception.ToString();
      Console.Error.WriteLine(exception);
    }
    string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(outputPath, json);
    Console.WriteLine(json);
    return exitCode;
  }

  private static void Send<TPacket>(NetworkStream stream, ProtocolProfile profile, TPacket packet)
      where TPacket : notnull {
    byte[] bytes = profile.Find(PacketDirection.ClientToServer, typeof(TPacket)).Encode(packet);
    stream.Write(bytes);
  }

  private static object Read(NetworkStream stream, ProtocolProfile profile, byte expected) {
    while (true) {
      (byte id, object packet) = ReadFrame(stream, profile);
      if (id == expected) {
        return packet;
      }
      // Controls can cause unsolicited section transfers before a placement response.
      Verify.That(expected is not (9 or 10) && id is 9 or 10,
          $"Expected packet {expected}, received {id}.");
    }
  }

  private static (byte Id, object Packet) ReadFrame(NetworkStream stream, ProtocolProfile profile) {
    byte[] header = new byte[3];
    stream.ReadExactly(header);
    int length = BinaryPrimitives.ReadUInt16LittleEndian(header) - header.Length;
    Verify.That(length >= 0, "The server returned an invalid frame length.");
    byte[] body = new byte[length];
    stream.ReadExactly(body);
    return (header[2], profile.Find(PacketDirection.ServerToClient, header[2]).Decode(body));
  }

  private static Packet10Tile TileAt(TileSectionPacket section, short x, short y) {
    return section.Tiles[(y - section.StartY) * section.Width + x - section.StartX];
  }
}
