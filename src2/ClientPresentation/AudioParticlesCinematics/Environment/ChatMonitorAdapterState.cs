namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class ChatMonitorAdapterState
{
  private IChatMonitorPort? _monitor;

  public bool IsAttached => _monitor is not null;

  public void Attach(IChatMonitorPort monitor)
  {
    ArgumentNullException.ThrowIfNull(monitor);
    _monitor = monitor;
  }

  public void Detach()
  {
    _monitor = null;
  }

  public void Publish(ChatMessage message)
  {
    (_monitor ?? throw new InvalidOperationException("No chat monitor is attached.")).Publish(message);
  }
}
