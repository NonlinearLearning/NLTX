namespace Terraria.Network.Session;

public sealed class NetworkConfigurationAdapter
{
  public bool TryApply(
    NetworkSessionConfigurationStateComponent state,
    NetworkSessionConfigurationInput input,
    out string error)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(input);

    return state.TryApply(input, out error);
  }

  public void Freeze(NetworkSessionConfigurationStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.Freeze();
  }
}
