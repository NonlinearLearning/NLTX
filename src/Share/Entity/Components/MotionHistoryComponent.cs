namespace EntityEcs.Components;

public struct MotionHistoryComponent
{
  public MotionHistoryKind Kind;
  public LocationComponent PreviousPosition;
  public VelocityComponent PreviousVelocity;
  public long? RecordedAtTick;

  public MotionHistoryKind HistoryKind
  {
    get => Kind;
    set => Kind = value;
  }

}
