using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text.Json;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class LiveMiningVerification {
  public static int Run(int port, string factsPath, string outputPath) {
    var result = new Dictionary<string, object> {
      ["port"] = port, ["helloVersion"] = "Terraria326", ["passed"] = false
    };
    int exitCode = 1;
    try {
      using JsonDocument factsDocument = JsonDocument.Parse(File.ReadAllText(factsPath));
      bool[] framed = factsDocument.RootElement.GetProperty("frameImportant").EnumerateArray()
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
      TileSectionPacket targetSection = sections.Single(section =>
          world.SpawnTileX >= section.StartX && world.SpawnTileX < section.StartX + section.Width
          && world.SpawnTileY >= section.StartY
          && world.SpawnTileY < section.StartY + section.Height);
      int index = Enumerable.Range(0, targetSection.Tiles.Length).First(index =>
          targetSection.Tiles[index].Active && targetSection.Tiles[index].Type is 0 or 1);
      short x = checked((short)(targetSection.StartX + index % targetSection.Width));
      short y = checked((short)(targetSection.StartY + index / targetSection.Width));
      Send(stream, profile, new PlayerSpawnPacket { Player = player, SpawnX = -1, SpawnY = -1 });
      _ = Read(stream, profile, 129);
      result["initialSections"] = sections.Count;
      result["tileX"] = x;
      result["tileY"] = y;
      Send(stream, profile, new PlayerControlsPacket {
        Player = player, Position = new PacketVector2(x * 16 + 8, y * 16 + 8)
      });
      const int hits = 60;
      int pingReplies = 0;
      for (int hit = 0; hit < hits; hit++) {
        Send(stream, profile, new ItemUseSoundPacket { Player = player });
        Send(stream, profile, new TileManipulationPacket { X = x, Y = y, TileOrWallType = 1 });
        Send(stream, profile, new SyncTilePickingPacket {
          Player = player, X = x, Y = y, TileType = 20
        });
        Send(stream, profile, new PlayerBuffsPacket {
          Player = player, BuffTypes = new ushort[] { (ushort)(1 + hit % 2), 3 }
        });
        if (hit % 16 == 0) {
          Send(stream, profile, new PingPacket());
          _ = Read(stream, profile, 154);
          pingReplies++;
        }
        Thread.Sleep(16);
      }
      TileSectionPacket afterHits = ReadSection(stream, profile, x, y);
      Verify.That(TileAt(afterHits, x, y).Active,
          "Sixty partial hits must leave the original block active.");
      result["partialHits"] = hits;
      result["soundPackets"] = hits;
      result["pickPackets"] = hits;
      result["buffPackets"] = hits;
      result["partialHitsPreservedTile"] = true;
      Send(stream, profile, new TileManipulationPacket { X = x, Y = y });
      var broken = (AreaTileChangePacket)Read(stream, profile, 20);
      Verify.That(!broken.Tiles[0].Active, "The final hit must return an inactive tile.");
      Send(stream, profile, new TileManipulationPacket { X = x, Y = y });
      var repeated = (AreaTileChangePacket)Read(stream, profile, 20);
      Verify.That(!repeated.Tiles[0].Active, "A repeated break must retain the inactive tile.");
      TileSectionPacket afterBreak = ReadSection(stream, profile, x, y);
      Verify.That(!TileAt(afterBreak, x, y).Active,
          "The post-break section must reflect the invalidated cache.");
      Send(stream, profile, new PingPacket());
      _ = Read(stream, profile, 154);
      result["latencyReplies"] = pingReplies + 1;
      result["finalBreakConfirmed"] = true;
      result["repeatedBreakConfirmed"] = true;
      result["sectionAfterBreakConfirmed"] = true;
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
    (byte id, object packet) = ReadFrame(stream, profile);
    Verify.That(id == expected, $"Expected packet {expected}, received {id}.");
    return packet;
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

  private static TileSectionPacket ReadSection(NetworkStream stream, ProtocolProfile profile,
      short x, short y) {
    Send(stream, profile, new RequestSectionPacket {
      SectionX = (ushort)(x / 200), SectionY = (ushort)(y / 150)
    });
    _ = Read(stream, profile, 9);
    return (TileSectionPacket)Read(stream, profile, 10);
  }

  private static Packet10Tile TileAt(TileSectionPacket section, short x, short y) {
    return section.Tiles[(y - section.StartY) * section.Width + x - section.StartX];
  }
}
