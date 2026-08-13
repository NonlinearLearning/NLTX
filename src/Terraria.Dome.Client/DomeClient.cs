using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Terraria.Dome.Transport;

namespace Terraria.Dome.Client;

public sealed class DomeClient : IDisposable
{
  private TcpClient? _client;
  private StreamReader? _reader;
  private StreamWriter? _writer;
  private bool _disposed;

  public async Task ConnectAsync(int port)
  {
    ThrowIfDisposed();
    if (_client is not null)
    {
      throw new InvalidOperationException("Client is already connected.");
    }

    TcpClient client = new();
    await client.ConnectAsync("127.0.0.1", port);
    NetworkStream stream = client.GetStream();
    _client = client;
    _reader = new StreamReader(stream, leaveOpen: true);
    _writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = true };
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _writer?.Dispose();
    _reader?.Dispose();
    _client?.Dispose();
  }

  public async Task<ServerSnapshot> SendAsync(ClientInputFrame frame)
  {
    ThrowIfDisposed();
    StreamReader reader = _reader ?? throw new InvalidOperationException("Client is not connected.");
    StreamWriter writer = _writer ?? throw new InvalidOperationException("Client is not connected.");
    await writer.WriteLineAsync(DomeFrameCodec.Serialize(frame));
    string? response = await reader.ReadLineAsync();
    if (response is null)
    {
      throw new IOException("Server closed the connection before responding.");
    }

    return DomeFrameCodec.Deserialize<ServerSnapshot>(response);
  }

  private void ThrowIfDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeClient));
    }
  }
}
