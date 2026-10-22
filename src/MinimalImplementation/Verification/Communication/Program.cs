using System.Net;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

if (args.Length != 0)
{
  Console.Error.WriteLine("This verifier does not accept arguments.");
  return 2;
}

try
{
  await RunCommunicationRoundTripAsync().ConfigureAwait(false);
  Console.WriteLine("COMM001 PASS");
  return 0;
}
catch (Exception exception)
{
  Console.Error.WriteLine($"COMM001 FAIL {exception.GetType().Name}: {exception.Message}");
  return 1;
}

static async Task RunCommunicationRoundTripAsync()
{
  Version4ProtocolFactsInput facts = CreateProtocolFacts();
  _ = Version4ProtocolFactsFactory.Create(facts);
  var profile = new Version4WireProfile(facts);
  SessionEpoch expectedEpoch = SessionEpoch.Create(7001);
  var received = NewCompletionSource<InboundVersion4Packet>();
  var closed = NewCompletionSource<ConnectionClosed>();
  int port = ReserveLoopbackPort();
  using var listener = new TcpListener(IPAddress.Loopback, port);
  listener.Start();

  using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
  using var client = new TcpClient();
  await client.ConnectAsync(IPAddress.Loopback, port, timeout.Token).ConfigureAwait(false);
  Socket serverSocket = await listener.AcceptSocketAsync(timeout.Token).ConfigureAwait(false);
  await using var connection = new PacketTcpConnection(
    serverSocket,
    expectedEpoch,
    (frame, cancellationToken) =>
    {
      InboundPacketEnvelope envelope = CreateEnvelope(frame);
      InboundVersion4Packet packet = profile.DecodeInbound(frame, envelope, false);
      received.TrySetResult(packet);
      return ValueTask.CompletedTask;
    },
    onClosed: connectionClosed =>
    {
      closed.TrySetResult(connectionClosed);
      return ValueTask.CompletedTask;
    });
  Task run = connection.RunAsync(timeout.Token);

  NetworkStream stream = client.GetStream();
  Version4EncodedPacket inboundEncoded = profile.Encode(
    WireDirection.ClientToServer,
    new PingPacket());
  byte[] inboundFrame = PacketFrameCodec.Encode(
    inboundEncoded.MessageId,
    inboundEncoded.Payload.Span);
  await stream.WriteAsync(inboundFrame, timeout.Token).ConfigureAwait(false);
  await stream.FlushAsync(timeout.Token).ConfigureAwait(false);

  InboundVersion4Packet inbound = await received.Task.WaitAsync(timeout.Token)
    .ConfigureAwait(false);
  Require(inbound.Handling == Version4PacketHandling.Typed,
    "The host did not classify the complete ping frame as typed.");
  Require(inbound.DecodedPacket is PingPacket,
    "The host did not decode the inbound ping packet.");
  Require(inbound.Envelope.Epoch == expectedEpoch,
    "The inbound envelope carried the wrong session epoch.");
  Require(inbound.Envelope.Sequence == new PacketSequence(1),
    "The first inbound frame did not receive sequence one.");

  Version4EncodedPacket outboundEncoded = profile.Encode(
    WireDirection.ServerToClient,
    new PingPacket());
  var outboundIntent = new OutboundPacketIntent(
    [expectedEpoch],
    null,
    outboundEncoded.MessageId,
    outboundEncoded.Payload,
    CommitVersion.Initial);
  SendReceipt receipt = await connection.SendAsync(outboundIntent, timeout.Token)
    .ConfigureAwait(false);
  Require(receipt.Epoch == expectedEpoch, "The outbound receipt has the wrong epoch.");
  Require(receipt.Certainty is TransportSendCertainty.Queued or
    TransportSendCertainty.LocallySent,
    $"The outbound receipt was not accepted: {receipt.Certainty}.");

  byte[] outboundFrame = await ReadFrameAsync(stream, timeout.Token).ConfigureAwait(false);
  Require(outboundFrame.AsSpan().SequenceEqual(
    PacketFrameCodec.Encode(outboundEncoded.MessageId, outboundEncoded.Payload.Span)),
    "The client received bytes different from the production outbound frame.");

  client.Client.Shutdown(SocketShutdown.Send);
  ConnectionClosed close = await closed.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
  Require(close.Epoch == expectedEpoch, "The close callback has the wrong session epoch.");
  await run.ConfigureAwait(false);
}

static InboundPacketEnvelope CreateEnvelope(InboundRawFrame frame)
{
  return new InboundPacketEnvelope(
    frame.Epoch,
    frame.Sequence,
    frame.ReceivedAt,
    frame.Frame.MessageId,
    frame.Frame.ModuleId,
    frame.Frame.FrameBytes,
    "Active",
    worldScope: null,
    entityGeneration: null,
    expectedRevision: null);
}

static Version4ProtocolFactsInput CreateProtocolFacts()
{
  const string facts = "8E5F2D0A3AA1E5A5E1AFC8E8D9CC7C0D84CB4B2E5BCEDB1B7AE9D49FE2BA88B1";
  return new Version4ProtocolFactsInput(
    new[] { true },
    new[] { false },
    PacketTileEntityCodecsV4.Create(),
    new[] { true },
    static (_, _, _) => 1,
    static _ => false,
    travelMerchantSlotCount: 40,
    tagEffectNpcSlotCount: 0,
    static _ => false,
    facts);
}

static int ReserveLoopbackPort()
{
  using var listener = new TcpListener(IPAddress.Loopback, 0);
  listener.Start();
  int port = ((IPEndPoint)listener.LocalEndpoint).Port;
  listener.Stop();
  return port;
}

static async Task<byte[]> ReadFrameAsync(
  NetworkStream stream,
  CancellationToken cancellationToken)
{
  byte[] lengthBytes = new byte[sizeof(ushort)];
  await stream.ReadExactlyAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
  ushort length = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
  Require(length >= PacketFrameCodec.MinimumFrameBytes,
    "The outbound frame length was below the protocol minimum.");
  byte[] frame = new byte[length];
  lengthBytes.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(
    frame.AsMemory(sizeof(ushort)),
    cancellationToken).ConfigureAwait(false);
  return frame;
}

static TaskCompletionSource<T> NewCompletionSource<T>()
{
  return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
}

static void Require(bool condition, string detail)
{
  if (!condition)
  {
    throw new InvalidOperationException(detail);
  }
}
