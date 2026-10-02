namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SpriteBatchStateComponent
{
  public bool IsActive { get; private set; }

  public uint FrameLease { get; private set; }

  public SpriteBatchStateInput State { get; private set; }

  public uint Revision { get; private set; }

  internal void Begin(uint frameLease, SpriteBatchStateInput state)
  {
    if (frameLease == 0 || IsActive)
    {
      throw new InvalidOperationException("The sprite batch state is already active or has no lease.");
    }

    if (string.IsNullOrWhiteSpace(state.SortMode) ||
      string.IsNullOrWhiteSpace(state.BlendState) ||
      string.IsNullOrWhiteSpace(state.SamplerState) ||
      string.IsNullOrWhiteSpace(state.DepthStencilState) ||
      string.IsNullOrWhiteSpace(state.RasterizerState) ||
      string.IsNullOrWhiteSpace(state.TransformToken))
    {
      throw new ArgumentException("Sprite batch state is incomplete.", nameof(state));
    }

    FrameLease = frameLease;
    State = state;
    IsActive = true;
    Revision++;
  }

  internal void End(uint frameLease)
  {
    if (!IsActive || FrameLease != frameLease)
    {
      throw new InvalidOperationException("The sprite batch lease is no longer valid.");
    }

    IsActive = false;
    FrameLease = 0;
    Revision++;
  }
}
