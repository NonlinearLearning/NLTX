namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// One located content problem.
/// </summary>
/// <remarks>
/// <paramref name="Source" /> is free text supplied by the infrastructure that read the unit, for
/// example <c>Item/items.armor.json:42</c>. The domain layer never interprets it, which
/// keeps this type free of any file model.
/// </remarks>
public readonly record struct LvcsDiagnostic
{
  public LvcsDiagnostic(
      LvcsDiagnosticSeverity severity,
      LvcsFamilyId family,
      string? id,
      string? field,
      string message,
      string? source)
  {
    if (string.IsNullOrWhiteSpace(message))
    {
      throw new ArgumentException("A diagnostic message is required.", nameof(message));
    }

    Severity = severity;
    Family = family;
    Id = id;
    Field = field;
    Message = message;
    Source = source;
  }

  public LvcsDiagnosticSeverity Severity { get; }

  public LvcsFamilyId Family { get; }

  /// <summary>Entry id this problem belongs to; null for family level problems.</summary>
  public string? Id { get; }

  /// <summary>Field name this problem belongs to; null when not field specific.</summary>
  public string? Field { get; }

  public string Message { get; }

  /// <summary>Origin reported by the source; null when unknown.</summary>
  public string? Source { get; }

  public override string ToString()
  {
    string where = Source ?? Family.ToString();
    string target = Id is null ? where : $"{where} [{Id}]";
    return Field is null
        ? $"{Severity}: {target}: {Message}"
        : $"{Severity}: {target}.{Field}: {Message}";
  }
}
