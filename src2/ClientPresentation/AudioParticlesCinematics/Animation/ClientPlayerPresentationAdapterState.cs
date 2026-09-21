namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public sealed class ClientPlayerPresentationAdapterState
{
  public object? ClientPlayer { get; private set; }

  public bool IsAttached => ClientPlayer is not null;

  public void Attach(object clientPlayer)
  {
    ArgumentNullException.ThrowIfNull(clientPlayer);
    ClientPlayer = clientPlayer;
  }

  public void Detach()
  {
    ClientPlayer = null;
  }
}
