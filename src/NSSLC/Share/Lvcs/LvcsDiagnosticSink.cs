namespace Terraria.Share.Lvcs;

/// <summary>
/// Collects diagnostics produced while parsing and projecting one family.
/// </summary>
/// <remarks>
/// A codec reports every problem it finds instead of throwing on the first one, so one run can
/// surface a full list. Exceptions remain reserved for programmer errors, not content errors.
/// </remarks>
public sealed class LvcsDiagnosticSink
{
  private readonly List<LvcsDiagnostic> _items = [];

  /// <summary>All diagnostics in report order.</summary>
  public IReadOnlyList<LvcsDiagnostic> Items => _items;

  /// <summary>True when at least one diagnostic is an error.</summary>
  public bool HasErrors
  {
    get
    {
      foreach (LvcsDiagnostic item in _items)
      {
        if (item.Severity == LvcsDiagnosticSeverity.Error)
        {
          return true;
        }
      }

      return false;
    }
  }

  public void Report(LvcsDiagnostic diagnostic) => _items.Add(diagnostic);

  public void Error(
      LvcsFamilyId family,
      string? id,
      string? field,
      string message,
      string? source = null)
  {
    _items.Add(new LvcsDiagnostic(
        LvcsDiagnosticSeverity.Error, family, id, field, message, source));
  }

  public void Warning(
      LvcsFamilyId family,
      string? id,
      string? field,
      string message,
      string? source = null)
  {
    _items.Add(new LvcsDiagnostic(
        LvcsDiagnosticSeverity.Warning, family, id, field, message, source));
  }
}
