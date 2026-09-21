namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class TimeLoggerDisplayFormatSet
{
  public TimeLoggerDisplayFormatSet()
  {
    PinnedCpu = new DiagnosticFormatPool("Pinned to CPU #{0}", 64);
    AssignedCpu = new DiagnosticFormatPool("Assigned CPU #{0}", 64);
    ProcessorThrottle = new DiagnosticFormatPool(
      "CPU Throttle/Boost {0:0}%", 200);
    ExpectedCpu = new DiagnosticFormatPool(
      "Expected CPU Usage {0:0}%", 200);
    TerrariaCpu = new DiagnosticFormatPool(
      "Terraria CPU Usage {0:0}%", 200);
    PendingCpu = new DiagnosticFormatPool("#Threads pending CPU {0}", 100);
    Percent = new DiagnosticFormatPool("{0,3:00}%", 100);
    Milliseconds = new DiagnosticFormatPool(
      "{0,5:F2}", 20, -10, 0.01);
    Integer = new DiagnosticFormatPool("{0,5}", 5000);
  }

  public DiagnosticFormatPool PinnedCpu { get; }

  public DiagnosticFormatPool AssignedCpu { get; }

  public DiagnosticFormatPool ProcessorThrottle { get; }

  public DiagnosticFormatPool ExpectedCpu { get; }

  public DiagnosticFormatPool TerrariaCpu { get; }

  public DiagnosticFormatPool PendingCpu { get; }

  public DiagnosticFormatPool Percent { get; }

  public DiagnosticFormatPool Milliseconds { get; }

  public DiagnosticFormatPool Integer { get; }
}
