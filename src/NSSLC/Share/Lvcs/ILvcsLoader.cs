namespace Terraria.Lvcs;

/// <summary>
/// Loads one content family: reads units, parses each, projects the batch.
/// </summary>
/// <remarks>
/// The contract is intentionally tiny. Everything that differs between families — inheritance,
/// baseline completion, reference checking, indexing — lives inside the codec, because only the
/// codec knows what its <typeparamref name="TValue" /> means.
/// </remarks>
public interface ILvcsLoader
{
  /// <summary>
  /// Loads the family described by <paramref name="codec" /> from <paramref name="source" />.
  /// </summary>
  /// <exception cref="ArgumentException">
  /// When the source family and the codec family disagree.
  /// </exception>
  LvcsLoadResult<TValue> Load<TRaw, TValue>(
      ILvcsCodec<TRaw, TValue> codec,
      ILvcsSource source)
      where TRaw : class
      where TValue : class;
}
