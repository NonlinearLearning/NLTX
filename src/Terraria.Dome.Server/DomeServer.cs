using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Simulation;
using Terraria.Dome.Transport;

namespace Terraria.Dome.Server;

public sealed class DomeServer : IDisposable
{
  private readonly CancellationTokenSource _cancellation = new();
  private readonly DomeSimulation _simulation = new();
  private Task? _acceptTask;
  private TcpListener? _listener;
  private bool _disposed;

  public int Port { get; private set; }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _cancellation.Cancel();
    _listener?.Stop();
    if (_acceptTask is not null)
    {
      try
      {
        _acceptTask.Wait(TimeSpan.FromSeconds(2));
      }
      catch (AggregateException)
      {
      }
    }

    _simulation.Dispose();
    _cancellation.Dispose();
  }

  public void Start(int port = 0)
  {
    ThrowIfStarted();

    _listener = new TcpListener(IPAddress.Loopback, port);
    _listener.Start();
    Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    _acceptTask = AcceptLoopAsync(_cancellation.Token);
  }

  private async Task AcceptLoopAsync(CancellationToken cancellationToken)
  {
    TcpListener listener = _listener ?? throw new InvalidOperationException("Server is not started.");
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
        using (client)
        {
          await HandleClientAsync(client, cancellationToken);
        }
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
    catch (SocketException) when (cancellationToken.IsCancellationRequested)
    {
    }
  }

  private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
  {
    PlayerHandle player = _simulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
    _simulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
    using NetworkStream stream = client.GetStream();
    using StreamReader reader = new(stream, leaveOpen: true);
    using StreamWriter writer = new(stream, leaveOpen: true) { AutoFlush = true };

    while (!cancellationToken.IsCancellationRequested)
    {
      string? line = await reader.ReadLineAsync(cancellationToken);
      if (line is null)
      {
        return;
      }

      ClientInputFrame frame = DomeFrameCodec.Deserialize<ClientInputFrame>(line);
      _simulation.Tick(new SimulationInputBatch(new PlayerInput(
        player,
        frame.MoveLeft,
        frame.MoveRight,
        frame.Jump,
        frame.Fire)));
      SimulationSnapshot snapshot = _simulation.CreateSnapshot();
      PlayerSnapshot playerSnapshot = snapshot.FindPlayer(player);
      NpcSnapshot npcSnapshot = snapshot.Npcs[0];
      ServerSnapshot response = new(
        snapshot.Tick,
        new ServerPlayerSnapshot(
          playerSnapshot.Position.X,
          playerSnapshot.Position.Y,
          playerSnapshot.Health),
        npcSnapshot.Health,
        snapshot.Projectiles.Count);
      await writer.WriteLineAsync(DomeFrameCodec.Serialize(response));
    }
  }

  private void ThrowIfStarted()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeServer));
    }

    if (_listener is not null)
    {
      throw new InvalidOperationException("Server is already started.");
    }
  }
}
