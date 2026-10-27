namespace Terraria.Diagnostics;

/// <summary>External sink for server diagnostic entries.</summary>
public interface IServerLogSink
{
  void Write(in ServerLogEntry entry);
}
