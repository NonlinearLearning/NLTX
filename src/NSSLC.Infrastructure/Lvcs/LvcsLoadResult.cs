namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Outcome of loading one family.
/// </summary>
/// <remarks>
/// Content errors are returned here rather than thrown, so a failed load can still report every
/// problem it found. <see cref="Value" /> is non-null exactly when <see cref="Succeeded" /> is true.
/// </remarks>
/// <typeparam name="TValue">Frozen domain value produced by the codec.</typeparam>
public sealed class LvcsLoadResult<TValue>
  where TValue : class
{
  private LvcsLoadResult(TValue? value, IReadOnlyList<LvcsDiagnostic> diagnostics)
  {
    Value = value;
    Diagnostics = diagnostics;
  }

  /// <summary>Loaded value, or null when loading failed.</summary>
  public TValue? Value { get; }

  /// <summary>Every diagnostic reported during the load.</summary>
  public IReadOnlyList<LvcsDiagnostic> Diagnostics { get; }

  /// <summary>True when the family was loaded and projected.</summary>
  public bool Succeeded => Value is not null;

  public static LvcsLoadResult<TValue> Success(
      TValue value,
      IReadOnlyList<LvcsDiagnostic> diagnostics)
  {
    ArgumentNullException.ThrowIfNull(value);
    ArgumentNullException.ThrowIfNull(diagnostics);
    return new LvcsLoadResult<TValue>(value, diagnostics);
  }

  public static LvcsLoadResult<TValue> Failure(IReadOnlyList<LvcsDiagnostic> diagnostics)
  {
    ArgumentNullException.ThrowIfNull(diagnostics);
    return new LvcsLoadResult<TValue>(null, diagnostics);
  }
}
