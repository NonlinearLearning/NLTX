namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileMergeCullMask(
  bool CullUp,
  bool CullDown,
  bool CullLeft,
  bool CullRight,
  bool CullUpLeft,
  bool CullUpRight,
  bool CullDownLeft,
  bool CullDownRight);
