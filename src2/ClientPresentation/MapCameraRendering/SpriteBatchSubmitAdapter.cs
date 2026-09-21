namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SpriteBatchSubmitAdapter
{
  private readonly ISpriteBatchSink _sink;

  public SpriteBatchSubmitAdapter(ISpriteBatchSink sink)
  {
    _sink = sink ?? throw new ArgumentNullException(nameof(sink));
  }

  public SpriteBatchSubmissionResult Submit(
    DrawCommandWorksetComponent workset,
    SpriteBatchStateComponent batchState,
    uint frameLease)
  {
    ArgumentNullException.ThrowIfNull(workset);
    ArgumentNullException.ThrowIfNull(batchState);
    if (!batchState.IsActive || batchState.FrameLease != frameLease)
    {
      throw new InvalidOperationException("The sprite batch state does not match the draw lease.");
    }

    IReadOnlyList<DrawCommand> commands = workset.Read(frameLease);
    foreach (DrawCommand command in commands)
    {
      _sink.Submit(command);
    }

    workset.Clear(frameLease);
    batchState.End(frameLease);
    return new SpriteBatchSubmissionResult(true, commands.Count, frameLease);
  }
}
