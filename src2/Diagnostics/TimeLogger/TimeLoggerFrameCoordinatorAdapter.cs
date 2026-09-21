namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class TimeLoggerFrameCoordinatorAdapter
{
  private readonly IDiagnosticLogOutput _output;
  private readonly TimeLoggerFrameState _state;

  public TimeLoggerFrameCoordinatorAdapter(
    IDiagnosticLogOutput output,
    int frameCount)
  {
    _output = output ?? throw new ArgumentNullException(nameof(output));
    _state = new TimeLoggerFrameState(frameCount);
  }

  public TimeLoggerFrameState State => _state;

  public TimeLogEntryState RegisterEntry(
    string name,
    Func<int, string> format,
    int budget)
  {
    TimeLogEntryState entry = new(name, format, budget, _state.FrameCount);
    _state.Register(entry);
    return entry;
  }

  public void StartNextFrame()
  {
    _state.Start();
    _output.Write($"Start of Frame #{_state.CurrentFrame}");
  }

  public void EndFrame()
  {
    _state.End();
    _output.Write($"End of Frame #{_state.CurrentFrame}");
  }
}
