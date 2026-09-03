using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;

await VerifyMalformedSessionIsolationAsync();
Console.WriteLine("PASS: malformed frames are rejected without stopping another session");

await VerifyProtocolFloodAsync();
Console.WriteLine("PASS: protocol flood is bounded and simulation progress continues");

await VerifyDisconnectAfterQueuedControlsAsync();
Console.WriteLine("PASS: disconnect after queued controls preserves simulation progress");

await VerifySlowReaderIsolationAsync();
Console.WriteLine("PASS: slow-reader session is isolated from a healthy session");

await VerifyRapidReconnectAsync();
Console.WriteLine("PASS: rapid reconnect preserves listener availability");

static async Task VerifyMalformedSessionIsolationAsync()
{
  using DomeServer server = new();
  server.Start();
  using TcpClient malformedClient = new();
  await malformedClient.ConnectAsync("127.0.0.1", server.Port);
  NetworkStream malformedStream = malformedClient.GetStream();
  await malformedStream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  _ = await ReadFrameAsync(malformedStream);
  await malformedStream.WriteAsync(new byte[] { 0x01, 0x00 });

  using TcpClient healthyClient = new();
  await healthyClient.ConnectAsync("127.0.0.1", server.Port);
  byte slot = await ActivateAsync(healthyClient.GetStream(), 2000, 0, "AfterMalformed");
  if (slot == 0)
  {
    throw new InvalidOperationException("Malformed session affected a later valid session.");
  }
}

static async Task VerifyProtocolFloodAsync()
{
  using DomeServer server = new();
  server.Start();
  using TcpClient client = new();
  await client.ConnectAsync("127.0.0.1", server.Port);
  NetworkStream stream = client.GetStream();
  byte playerSlot = await ActivateAsync(stream, 2000, 0, "Flood");
  byte[] controlFrame = TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    playerSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    2000.0f,
    0.0f);
  for (int index = 0; index < DomeServer.MaximumPendingProtocolCommands * 2; index++)
  {
    await stream.WriteAsync(controlFrame);
  }

  await Task.Delay(TimeSpan.FromMilliseconds(100));
  if (server.PendingProtocolCommandCount > DomeServer.MaximumPendingProtocolCommands ||
      server.DroppedProtocolCommandCount == 0)
  {
    throw new InvalidOperationException("Protocol flood exceeded the bounded command queue.");
  }

  long tickBefore = server.LatestSnapshot.Tick;
  await Task.Delay(TimeSpan.FromMilliseconds(100));
  if (server.LatestSnapshot.Tick <= tickBefore || server.LatestSnapshot.Players.Count != 1)
  {
    throw new InvalidOperationException("Protocol flood stopped simulation progress.");
  }
}

static async Task VerifyDisconnectAfterQueuedControlsAsync()
{
  using DomeServer server = new();
  server.Start();
  using TcpClient client = new();
  await client.ConnectAsync("127.0.0.1", server.Port);
  NetworkStream stream = client.GetStream();
  byte playerSlot = await ActivateAsync(stream, 2000, 0, "DisconnectAfterInput");
  byte[] controlFrame = TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    playerSlot,
    MoveLeft: false,
    MoveRight: true,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    2000.0f,
    0.0f);
  for (int index = 0; index <= DomeServer.MaximumProtocolCommandsPerTick; index++)
  {
    await stream.WriteAsync(controlFrame);
  }

  client.Close();
  await WaitForPlayerCountAsync(server, expectedCount: 0, TimeSpan.FromSeconds(3));
  long tickBefore = server.LatestSnapshot.Tick;
  await Task.Delay(TimeSpan.FromMilliseconds(100));
  if (server.SimulationFault is not null || server.LatestSnapshot.Tick <= tickBefore)
  {
    throw new InvalidOperationException(
      "Disconnect after queued controls stopped simulation progress. " +
      $"Tick={server.LatestSnapshot.Tick}, SimulationFault={server.SimulationFault}.");
  }
}

static async Task VerifySlowReaderIsolationAsync()
{
  using DomeServer server = new();
  const int replicationPressureSignCount = 5000;
  string replicationPressureText = new('x', 100);
  for (int index = 0; index < replicationPressureSignCount; index++)
  {
    _ = server.CreateSign(
      2000 + index % 200,
      150 + index % 150,
      replicationPressureText);
  }

  server.Start();
  using TcpClient healthyClient = new();
  await healthyClient.ConnectAsync("127.0.0.1", server.Port);
  NetworkStream healthyStream = healthyClient.GetStream();
  byte healthySlot = await ActivateAsync(healthyStream, 100, 0, "Healthy");
  await healthyStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    healthySlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    0.0f,
    0.0f));
  await Task.Delay(TimeSpan.FromMilliseconds(100));
  using CancellationTokenSource healthyCancellation = new();
  Task healthyDrain = DrainFramesAsync(healthyStream, healthyCancellation.Token);

  using TcpClient slowClient = new();
  slowClient.ReceiveBufferSize = 512;
  await slowClient.ConnectAsync("127.0.0.1", server.Port);
  NetworkStream slowStream = slowClient.GetStream();
  byte slowSlot;
  try
  {
    slowSlot = await ActivateAsync(slowStream, 2000, 0, "Slow");
  }
  catch (TimeoutException exception)
  {
    throw new TimeoutException(
      "Slow reader did not complete activation. " +
      $"Players={server.LatestSnapshot.Players.Count}, " +
      $"PendingCommands={server.PendingProtocolCommandCount}, " +
      $"SimulationFault={server.SimulationFault}, " +
      $"SessionFault={server.LastSessionFault}.",
      exception);
  }

  await slowStream.WriteAsync(TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
    slowSlot,
    MoveLeft: false,
    MoveRight: false,
    Jump: false,
    UseItem: false,
    FacingRight: true,
    SelectedItem: 0),
    2000.0f,
    0.0f));

  await WaitForPlayerCountAsync(server, expectedCount: 1, TimeSpan.FromSeconds(5));

  long tickBefore = server.LatestSnapshot.Tick;
  await Task.Delay(TimeSpan.FromMilliseconds(100));
  if (server.LatestSnapshot.Tick <= tickBefore || server.LatestSnapshot.Players.Count != 1)
  {
    throw new InvalidOperationException(
      "Slow reader blocked simulation or disconnected the healthy peer. " +
      $"Tick={server.LatestSnapshot.Tick}, SimulationFault={server.SimulationFault}, " +
      $"SessionFault={server.LastSessionFault}.");
  }

  healthyCancellation.Cancel();
  await healthyDrain;
}

static async Task VerifyRapidReconnectAsync()
{
  using DomeServer server = new();
  server.Start();
  for (int index = 0; index < 4; index++)
  {
    using TcpClient reconnectClient = new();
    await reconnectClient.ConnectAsync("127.0.0.1", server.Port);
    NetworkStream reconnectStream = reconnectClient.GetStream();
    await reconnectStream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
    TerrariaFrame setUserSlot;
    try
    {
      setUserSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(reconnectStream));
    }
    catch (TimeoutException exception)
    {
      throw new TimeoutException($"Rapid reconnect {index + 1} did not receive SetUserSlot.", exception);
    }
    if (setUserSlot.MessageId != TerrariaMessageId.SetUserSlot)
    {
      throw new InvalidOperationException("Rapid reconnect did not receive a user-slot frame.");
    }
  }

  using TcpClient finalClient = new();
  await finalClient.ConnectAsync("127.0.0.1", server.Port);
  byte finalSlot = await ActivateAsync(finalClient.GetStream(), 2000, 0, "Final");
  if (finalSlot != 5)
  {
    throw new InvalidOperationException("Rapid reconnect did not preserve monotonic session slots.");
  }
}

static async Task<byte> ActivateAsync(
  NetworkStream stream,
  short spawnX,
  short spawnY,
  string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  TerrariaColor color = new(0, 0, 0);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    userSlot.Payload.Span[0], 0, 0, 0.0f, 0, name, 0, 0, 0,
    color, color, color, color, color, color, color, 0, 0, 0)));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    Guid.NewGuid().ToString("D"))));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(spawnX, spawnY, 0)));
  _ = await ReadFrameAsync(stream);
  for (int index = 0; index < 15; index++)
  {
    _ = await ReadFrameAsync(stream);
  }

  _ = await ReadFrameAsync(stream);
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    userSlot.Payload.Span[0], spawnX, spawnY, 0, 0, 0, 0, 0)));
  bool receivedFinishedConnecting = false;
  const int maximumActivationFrames = 10_000;
  for (int index = 0; index < maximumActivationFrames; index++)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
    if (frame.MessageId == TerrariaMessageId.FinishedConnectingToServer)
    {
      receivedFinishedConnecting = true;
      break;
    }
  }

  if (!receivedFinishedConnecting)
  {
    throw new InvalidOperationException(
      "Server did not complete the player projection. " +
      $"ReadLimit={maximumActivationFrames}.");
  }

  return userSlot.Payload.Span[0];
}

static async Task WaitForPlayerCountAsync(
  DomeServer server,
  int expectedCount,
  TimeSpan timeout)
{
  DateTime deadline = DateTime.UtcNow + timeout;
  while (DateTime.UtcNow < deadline)
  {
    if (server.LatestSnapshot.Players.Count == expectedCount)
    {
      return;
    }

    await Task.Delay(25);
  }

  throw new TimeoutException($"Expected {expectedCount} players, but found " +
    $"{server.LatestSnapshot.Players.Count}.");
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  try
  {
    byte[] prefix = new byte[2];
    await stream.ReadExactlyAsync(prefix, timeout.Token);
    int frameLength = prefix[0] | prefix[1] << 8;
    if (frameLength < 2)
    {
      throw new InvalidDataException("The peer sent an invalid frame length.");
    }

    byte[] frame = new byte[frameLength];
    prefix.CopyTo(frame, 0);
    await stream.ReadExactlyAsync(frame.AsMemory(2), timeout.Token);
    return frame;
  }
  catch (OperationCanceledException exception)
  {
    throw new TimeoutException("Timed out while waiting for a server frame.", exception);
  }
}

static async Task DrainFramesAsync(NetworkStream stream, CancellationToken cancellationToken)
{
  try
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      _ = await ReadFrameAsync(stream);
    }
  }
  catch (IOException)
  {
  }
  catch (OperationCanceledException)
  {
  }
}
