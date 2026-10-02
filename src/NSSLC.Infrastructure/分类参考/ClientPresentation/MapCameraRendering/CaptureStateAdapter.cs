namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class CaptureStateAdapter
{
  private readonly ICaptureStateSource? _source;

  public CaptureStateAdapter(ICaptureStateSource? source = null)
  {
    _source = source;
  }

  public CaptureStateSnapshot Read()
  {
    return _source is null
      ? new CaptureStateSnapshot(false, false)
      : new CaptureStateSnapshot(_source.IsCapturing, true);
  }
}
