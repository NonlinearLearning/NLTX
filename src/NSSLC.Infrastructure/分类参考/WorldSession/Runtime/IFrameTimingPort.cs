namespace Terraria.WorldSession.Runtime;

public interface IFrameTimingPort
{
  FrameTimingInput ReadFrameTime();

  bool ReadGlobalPause();
}
