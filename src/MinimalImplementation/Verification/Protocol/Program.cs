using System.Net;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

if (!TryReadArguments(args, out string scenario, out string outputRoot))
{
  Console.Error.WriteLine(
    "Usage: --scenario SC001|SC002 --output-root <Build/diagnostics directory>");
  return 2;
}

Directory.CreateDirectory(outputRoot);
string result = scenario switch
{
  "SC001" => await RunSc001Async(),
  "SC002" => RunSc002(),
  _ => throw new InvalidOperationException($"Unknown scenario '{scenario}'.")
};

string resultPath = Path.Combine(outputRoot, $"{scenario}-result.md");
File.WriteAllText(resultPath, result);
Console.WriteLine($"{scenario} PASS");
Console.WriteLine($"Evidence: {resultPath}");
return 0;

static async Task<string> RunSc001Async()
{
  byte[] frameA = [0x07, 0x00, 0x9F, 0x02, 0x00, 0x03, 0x00];
  byte[] frameB = [0x05, 0x00, 0x26, 0x01, 0x70];
  byte[] input = [.. frameA, .. frameB];
  var received = new List<RawPacketFrame>();
  var epoch = SessionEpoch.Create(9001);
  var options = new PacketTcpConnectionOptions
  {
    ReceiveBufferBytes = 32,
    MaximumReceiveBufferedBytes = 64 * 1024,
    MaximumPendingSendBytes = 64 * 1024,
    MaximumPendingSendFrames = 8,
    ReceiveNoProgressDeadline = TimeSpan.FromSeconds(2),
    SendDeadline = TimeSpan.FromSeconds(2)
  };

  var listener = new TcpListener(IPAddress.Loopback, 0);
  listener.Start();
  var receiveChunks = new List<int>();
  try
  {
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    using Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    await client.ConnectAsync(IPAddress.Loopback, port, cancellation.Token);
    using Socket serverSocket = await listener.AcceptSocketAsync(cancellation.Token);
    int[] fixedChunks = [1, 2, 9];
    int chunkIndex = 0;

    async ValueTask<int> ReceiveFixedChunkAsync(Memory<byte> buffer, CancellationToken token)
    {
      if (chunkIndex == fixedChunks.Length)
      {
        int trailing = await serverSocket.ReceiveAsync(buffer, SocketFlags.None, token);
        Require(trailing == 0, "SC001 received bytes after the fixed input stream.");
        return 0;
      }

      int targetBytes = fixedChunks[chunkIndex];
      Require(buffer.Length >= targetBytes, "SC001 receive buffer cannot fit the fixed chunk.");
      int receivedBytes = 0;
      while (receivedBytes < targetBytes)
      {
        int count = await serverSocket.ReceiveAsync(
          buffer.Slice(receivedBytes, targetBytes - receivedBytes),
          SocketFlags.None,
          token);
        Require(count > 0, "SC001 reached EOF before the fixed chunk was complete.");
        receivedBytes += count;
      }

      chunkIndex++;
      receiveChunks.Add(receivedBytes);
      return receivedBytes;
    }

    var connection = new PacketTcpConnection(
      serverSocket,
      epoch,
      (frame, _) =>
      {
        received.Add(frame.Frame);
        return ValueTask.CompletedTask;
      },
      options,
      receiveBytes: ReceiveFixedChunkAsync);
    Task run = connection.RunAsync(cancellation.Token);
    try
    {
      await SendAllAsync(client, input.AsMemory(0, 1), cancellation.Token);
      await WaitForBufferedBytesAsync(connection, 1, cancellation.Token);
      Require(received.Count == 0, "SC001 emitted a frame from one header byte.");
      await SendAllAsync(client, input.AsMemory(1, 2), cancellation.Token);
      await WaitForBufferedBytesAsync(connection, 3, cancellation.Token);
      Require(received.Count == 0, "SC001 emitted a frame before its body arrived.");
      await SendAllAsync(client, input.AsMemory(3), cancellation.Token);
      client.Shutdown(SocketShutdown.Send);
      await run.ConfigureAwait(false);
    }
    finally
    {
      await connection.DisposeAsync().ConfigureAwait(false);
      await run.ConfigureAwait(false);
    }
  }
  finally
  {
    listener.Stop();
  }

  Require(receiveChunks.SequenceEqual(new[] { 1, 2, 9 }),
    "SC001 assembler input chunks were not exactly 1, 2, and 9 bytes.");
  Require(received.Count == 2, "SC001 expected exactly two complete frames.");
  Require(received[0].MessageId == 159, "SC001 first frame id mismatch.");
  Require(received[1].MessageId == 38, "SC001 second frame id mismatch.");
  Require(received[0].TotalLength == frameA.Length, "SC001 first frame length mismatch.");
  Require(received[1].TotalLength == frameB.Length, "SC001 second frame length mismatch.");
  Require(received[0].FrameBytes.Span.SequenceEqual(frameA), "SC001 first frame bytes changed.");
  Require(received[1].FrameBytes.Span.SequenceEqual(frameB), "SC001 second frame bytes changed.");
  return """
# SC001 result

Status: PASS

Input: fixed Version4 frames 159 (`07 00 9F 02 00 03 00`) and 38
(`05 00 26 01 70`) were written to one loopback connection in chunks of 1, 2,
and 9 bytes.

The scenario receive adapter gathers exactly 1, 2, and 9 bytes from the real
socket before each production assembler append. It asserts these input sizes,
observes the first two incomplete prefixes, and checks EOF after the fixed
stream; it does not infer receive boundaries from SendAsync calls.

Actual: the production `PacketTcpConnection` and `PacketFrameAssembler` emitted
exactly two frames in order, preserving both complete byte sequences and no
partial or third frame. The client half closed and the connection completed and
was disposed before the scenario returned.

This proves framing and ordering only; it does not prove admission or gameplay
processing.
""";
}

static string RunSc002()
{
  const string facts = "8E5F2D0A3AA1E5A5E1AFC8E8D9CC7C0D84CB4B2E5BCEDB1B7AE9D49FE2BA88B1";
  Version4ProtocolFactsInput factsInput = new(
    new[] { true },
    new[] { false },
    PacketTileEntityCodecsV4.Create(),
    new[] { true },
    static (_, _, _) => 1,
    static _ => false,
    40,
    0,
    static _ => false,
    facts);
  InitializeProtocolFacts(factsInput);
  var profile = new Version4WireProfile(factsInput);
  var packet = new SyncPlayerPacket
  {
    Player = 2,
    SkinVariant = 1,
    VoiceVariant = 3,
    VoicePitchOffset = 0.5f,
    Hair = 9,
    Name = "甲",
    HairDye = 4,
    HiddenAccessories = 0x0123,
    HideMisc = 2,
    HairColor = new PacketRgb(1, 2, 3),
    SkinColor = new PacketRgb(4, 5, 6),
    EyeColor = new PacketRgb(7, 8, 9),
    ShirtColor = new PacketRgb(10, 11, 12),
    UnderShirtColor = new PacketRgb(13, 14, 15),
    PantsColor = new PacketRgb(16, 17, 18),
    ShoeColor = new PacketRgb(19, 20, 21),
    DifficultyAndAccessoryFlags = 5,
    BiomeAndCartFlags = 0x11,
    PermanentUpgradeFlags = 0x43
  };
  byte[] expected =
  [
    0x2B, 0x00, 0x04, 0x02, 0x01, 0x03, 0x00, 0x00, 0x00, 0x3F, 0x09,
    0x03, 0xE7, 0x94, 0xB2, 0x04, 0x23, 0x01, 0x02, 0x01, 0x02, 0x03,
    0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
    0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x05, 0x11, 0x43
  ];
  Version4EncodedPacket encoded = profile.Encode(WireDirection.ServerToClient, packet);
  byte[] actual = PacketFrameCodec.Encode(encoded.MessageId, encoded.Payload.Span);
  Require(actual.SequenceEqual(expected), "SC002 writer bytes differ from the independent fixture.");
  object decoded = profile.Decode(WireDirection.ClientToServer, expected[2], expected.AsMemory(3));
  Require(decoded is SyncPlayerPacket, "SC002 reader did not decode the fixture.");
  SyncPlayerPacket actualPacket = (SyncPlayerPacket)decoded;
  Require(actualPacket.Player == packet.Player, "SC002 player field changed.");
  Require(actualPacket.SkinVariant == packet.SkinVariant, "SC002 skin field changed.");
  Require(actualPacket.VoiceVariant == packet.VoiceVariant, "SC002 voice field changed.");
  Require(actualPacket.VoicePitchOffset == packet.VoicePitchOffset, "SC002 pitch field changed.");
  Require(actualPacket.Hair == packet.Hair, "SC002 hair field changed.");
  Require(actualPacket.Name == packet.Name, "SC002 name field changed.");
  Require(actualPacket.HairDye == packet.HairDye, "SC002 hair-dye field changed.");
  Require(actualPacket.HiddenAccessories == packet.HiddenAccessories, "SC002 hidden-accessory field changed.");
  Require(actualPacket.HideMisc == packet.HideMisc, "SC002 hide-misc field changed.");
  Require(actualPacket.HairColor == packet.HairColor, "SC002 hair color changed.");
  Require(actualPacket.SkinColor == packet.SkinColor, "SC002 skin color changed.");
  Require(actualPacket.EyeColor == packet.EyeColor, "SC002 eye color changed.");
  Require(actualPacket.ShirtColor == packet.ShirtColor, "SC002 shirt color changed.");
  Require(actualPacket.UnderShirtColor == packet.UnderShirtColor, "SC002 undershirt color changed.");
  Require(actualPacket.PantsColor == packet.PantsColor, "SC002 pants color changed.");
  Require(actualPacket.ShoeColor == packet.ShoeColor, "SC002 shoe color changed.");
  Require(
    actualPacket.DifficultyAndAccessoryFlags == packet.DifficultyAndAccessoryFlags,
    "SC002 difficulty/accessory flags changed.");
  Require(actualPacket.BiomeAndCartFlags == packet.BiomeAndCartFlags, "SC002 biome/cart flags changed.");
  Require(actualPacket.PermanentUpgradeFlags == packet.PermanentUpgradeFlags, "SC002 upgrade flags changed.");
  return """
# SC002 result

Status: PASS

Input: independent Version4 source-derived Packet 4 fixture with player 2,
voice variant 3, pitch `0.5`, UTF-8 name `甲`, seven RGB triples, and flags
`05 11 43`.

Actual: the production Version4 generated binding emitted the exact 43-byte
frame (`2B 00 04` plus the 40-byte body), and the same profile accepted the
hand-written body with the expected name and pitch. The expected bytes were
constructed in this verifier and were not obtained from the writer.

This proves the selected Packet 4 wire layout only; it does not prove player
authorization, state mutation, or client processing.
""";
}

static void InitializeProtocolFacts(Version4ProtocolFactsInput facts)
{
  if (ProtocolInputs.IsInitialized)
  {
    throw new InvalidOperationException("SC002 protocol facts were initialized more than once.");
  }

  _ = Version4ProtocolFactsFactory.Create(facts);
}

static bool TryReadArguments(
  IReadOnlyList<string> args,
  out string scenario,
  out string outputRoot)
{
  scenario = string.Empty;
  outputRoot = string.Empty;
  if (args.Count != 4
    || args[0] != "--scenario"
    || args[2] != "--output-root"
    || string.IsNullOrWhiteSpace(args[1])
    || string.IsNullOrWhiteSpace(args[3]))
  {
    return false;
  }

  scenario = args[1];
  outputRoot = Path.GetFullPath(args[3]);
  return scenario is "SC001" or "SC002";
}

static async Task WaitForBufferedBytesAsync(
  PacketTcpConnection connection,
  int expectedBytes,
  CancellationToken cancellationToken)
{
  while (connection.BufferedReceiveBytes < expectedBytes)
  {
    await Task.Delay(TimeSpan.FromMilliseconds(5), cancellationToken);
  }

  Require(
    connection.BufferedReceiveBytes == expectedBytes,
    $"SC001 expected {expectedBytes} bytes buffered before writing the next chunk.");
}

static async Task SendAllAsync(
  Socket socket,
  ReadOnlyMemory<byte> bytes,
  CancellationToken cancellationToken)
{
  while (!bytes.IsEmpty)
  {
    int sent = await socket.SendAsync(bytes, SocketFlags.None, cancellationToken);
    Require(sent > 0, "SC001 socket did not make send progress.");
    bytes = bytes[sent..];
  }
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
