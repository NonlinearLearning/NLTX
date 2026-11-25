namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Wraps a concrete codec as a non generic descriptor.
/// </summary>
/// <remarks>
/// The loader call is captured as a delegate at construction, so dispatch never needs reflection
/// or <c>Activator</c>. <see cref="Load" /> boxes the projected value; the registry casts it back
/// to <see cref="ValueType" />, which is the same <c>TValue</c> the codec was registered with.
/// </remarks>
internal sealed class LvcsCodecDescriptor<TRaw, TValue> : ILvcsCodecDescriptor
  where TRaw : class
  where TValue : class
{
  private readonly ILvcsCodec<TRaw, TValue> _codec;
  private readonly ILvcsLoader _loader;
  private readonly string[] _extensions;

  internal LvcsCodecDescriptor(ILvcsCodec<TRaw, TValue> codec, ILvcsLoader loader)
  {
    ArgumentNullException.ThrowIfNull(codec);
    ArgumentNullException.ThrowIfNull(loader);
    _codec = codec;
    _loader = loader;

    IReadOnlyList<string> declared = codec.Extensions
        ?? throw new InvalidOperationException(
            $"Codec for {typeof(TValue).Name} returned a null extension list.");
    string[] normalized = new string[declared.Count];
    for (int index = 0; index < declared.Count; index++)
    {
      string extension = declared[index];
      if (string.IsNullOrWhiteSpace(extension) || extension[0] != '.')
      {
        throw new InvalidOperationException(
            $"Codec for {typeof(TValue).Name} declared extension '{extension}'; " +
            "extensions must be non empty and start with a dot.");
      }

      normalized[index] = extension;
    }

    _extensions = normalized;
  }

  public LvcsFamilyId Family => _codec.Family;

  public Type ValueType => typeof(TValue);

  public IReadOnlyList<string> Extensions => _extensions;

  public bool Handles(string extension)
  {
    if (string.IsNullOrWhiteSpace(extension))
    {
      return false;
    }

    foreach (string candidate in _extensions)
    {
      if (string.Equals(candidate, extension, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
    }

    return false;
  }

  public object? Load(ILvcsSource source, out IReadOnlyList<LvcsDiagnostic> diagnostics)
  {
    LvcsLoadResult<TValue> result = _loader.Load(_codec, source);
    diagnostics = result.Diagnostics;
    return result.Succeeded ? result.Value : null;
  }
}
