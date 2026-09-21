using Terraria.WorldGeneration.Support;

namespace Terraria.WorldGeneration.Adapters;

public sealed class RoomCheckFeedbackAdapter
{
  private readonly GenerationFeedbackPort _port;

  public RoomCheckFeedbackAdapter(GenerationFeedbackPort port)
  {
    _port = port ?? throw new ArgumentNullException(nameof(port));
  }

  public void BeginSpread(int x, int y)
  {
    _port.BeginSpread();
  }

  public void StartedInASolidTile(int x, int y)
  {
    _port.Record(GenerationFeedbackReason.StartedInASolidTile, x, y, iteration: 0);
  }

  public void TooCloseToWorldEdge(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.TooCloseToWorldEdge, x, y, iteration);
  }

  public void AnyBlockScannedHere(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.AnyBlockScannedHere, x, y, iteration);
  }

  public void RoomTooBig(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.RoomTooBig, x, y, iteration);
  }

  public void BlockingWall(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.BlockingWall, x, y, iteration);
  }

  public void BlockingOpenGate(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.BlockingOpenGate, x, y, iteration);
  }

  public void Stinkbug(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.Stinkbug, x, y, iteration);
  }

  public void EchoStinkbug(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.EchoStinkbug, x, y, iteration);
  }

  public void MissingAWall(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.MissingAWall, x, y, iteration);
  }

  public void UnsafeWall(int x, int y, int iteration)
  {
    _port.Record(GenerationFeedbackReason.UnsafeWall, x, y, iteration);
  }

  public void EndSpread()
  {
    _port.EndSpread();
  }
}
