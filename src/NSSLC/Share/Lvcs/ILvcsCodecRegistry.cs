namespace Terraria.Lvcs;

/// <summary>
/// Maps a domain value type — optionally narrowed by file extension — to the codec that produces
/// it.
/// </summary>
/// <remarks>
/// Dispatch follows two rules borrowed from mature asset systems, with one deliberate difference:
/// <list type="bullet">
/// <item>
/// The target type is the primary signal; an extension only narrows the candidates. A stable
/// domain type must not change meaning when the on disk format changes.
/// </item>
/// <item>
/// Ambiguity is an error. Where Bevy silently keeps the most recently registered loader, this
/// registry refuses the second registration and refuses to guess at lookup time, because implicit
/// order would make the outcome depend on assembly registration order.
/// </item>
/// </list>
/// </remarks>
public interface ILvcsCodecRegistry
{
  /// <summary>Number of registered codecs.</summary>
  int Count { get; }

  /// <summary>
  /// Registers a codec.
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// A codec already produces <typeparamref name="TValue" /> with an overlapping extension set.
  /// </exception>
  void Register<TRaw, TValue>(ILvcsCodec<TRaw, TValue> codec)
      where TRaw : class
      where TValue : class;

  /// <summary>
  /// Resolves the single codec producing <typeparamref name="TValue" />.
  /// </summary>
  /// <param name="descriptor">The resolved codec, or null when resolution failed.</param>
  /// <param name="reason">Why resolution failed, or null on success.</param>
  bool TryResolve<TValue>(out ILvcsCodecDescriptor? descriptor, out string? reason)
      where TValue : class;

  /// <summary>
  /// Resolves the single codec producing <typeparamref name="TValue" /> that also reads
  /// <paramref name="extension" />.
  /// </summary>
  /// <param name="descriptor">The resolved codec, or null when resolution failed.</param>
  /// <param name="reason">Why resolution failed, or null on success.</param>
  bool TryResolve<TValue>(
      string extension,
      out ILvcsCodecDescriptor? descriptor,
      out string? reason)
      where TValue : class;
}
