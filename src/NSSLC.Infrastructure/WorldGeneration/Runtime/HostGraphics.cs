using System;

namespace NSSLC.WorldGeneration.HostGraphics;

/// <summary>Legacy presentation signatures; the memory generator has no graphics host.</summary>
public sealed class Texture2D { }
public sealed class SpriteBatch { }
public sealed class Asset<T> {
  public T Value => throw new NotSupportedException("A graphics host is required.");
}
