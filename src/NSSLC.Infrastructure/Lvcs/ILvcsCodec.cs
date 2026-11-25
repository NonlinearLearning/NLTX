namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Turns raw units of one family into one frozen domain value.
/// </summary>
/// <remarks>
/// Failure is signalled by throwing <see cref="LvcsContentException" />, not by returning null.
/// That matches how most third party loaders behave — they fill a caller supplied object and
/// throw when the payload is malformed — so a codec can delegate to such a loader without an
/// adapter. The loader is the single place that converts exceptions into diagnostics.
/// <para>
/// <see cref="Parse" /> sees one unit and must not consult siblings. <see cref="Project" /> sees
/// the whole batch and is therefore the only place where cross entry validation (references,
/// cycles, uniqueness) can happen.
/// </para>
/// <para>
/// <see cref="ValueType" /> and <see cref="Extensions" /> describe what this codec can produce and
/// read, so a registry can dispatch without knowing concrete codec types. They are capabilities
/// reported by the codec itself, not external configuration: adding a format changes only the
/// codec.
/// </para>
/// </remarks>
/// <typeparam name="TRaw">Intermediate representation produced by parsing.</typeparam>
/// <typeparam name="TValue">Frozen domain value produced by projection.</typeparam>
public interface ILvcsCodec<TRaw, TValue>
  where TRaw : class
  where TValue : class
{
  /// <summary>Family this codec loads.</summary>
  LvcsFamilyId Family { get; }

  /// <summary>Domain value type this codec produces.</summary>
  Type ValueType => typeof(TValue);

  /// <summary>
  /// File extensions this codec reads, written with a leading dot and compared case
  /// insensitively (for example <c>.json</c>).
  /// </summary>
  IReadOnlyList<string> Extensions { get; }

  /// <summary>
  /// Parses one unit into one or more entries.
  /// </summary>
  /// <exception cref="LvcsContentException">The unit is not valid content.</exception>
  IReadOnlyList<TRaw> Parse(ILvcsUnit unit, LvcsDiagnosticSink diagnostics);

  /// <summary>
  /// Projects the whole parsed batch into one frozen value, performing cross entry validation and
  /// domain construction.
  /// </summary>
  /// <exception cref="LvcsContentException">The batch is not valid content.</exception>
  TValue Project(IReadOnlyList<TRaw> raws, LvcsDiagnosticSink diagnostics);
}
