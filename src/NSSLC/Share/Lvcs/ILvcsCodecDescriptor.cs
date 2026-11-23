namespace Terraria.Lvcs;

/// <summary>
/// Non generic view of a codec, so a registry can hold codecs of differing type arguments.
/// </summary>
/// <remarks>
/// A descriptor exposes only what dispatch needs. Loading still happens through the generic
/// <see cref="ILvcsCodec{TRaw,TValue}" />: <see cref="Load" /> closes over that interface and
/// returns the result as <see cref="object" />, which the registry casts back to
/// <see cref="ValueType" />. The cast is safe because <see cref="ValueType" /> is derived from the
/// same <c>TValue</c> the codec was registered with.
/// </remarks>
public interface ILvcsCodecDescriptor
{
  /// <summary>Family this codec loads.</summary>
  LvcsFamilyId Family { get; }

  /// <summary>Domain value type this codec produces.</summary>
  Type ValueType { get; }

  /// <summary>File extensions this codec reads, with a leading dot.</summary>
  IReadOnlyList<string> Extensions { get; }

  /// <summary>
  /// Loads the family through this codec and returns the projected value boxed as
  /// <see cref="object" />, or null when loading failed. Diagnostics are available through
  /// <paramref name="diagnostics" />.
  /// </summary>
  object? Load(ILvcsSource source, out IReadOnlyList<LvcsDiagnostic> diagnostics);

  /// <summary>True when this codec declares the given extension.</summary>
  bool Handles(string extension);
}
