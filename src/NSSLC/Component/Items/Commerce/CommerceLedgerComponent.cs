using System.Collections.Generic;

namespace Terraria.Items.Commerce;

public sealed class CommerceLedgerComponent
{
  private readonly List<CommerceLedgerEntry> _entries;

  public CommerceLedgerComponent(
    IReadOnlyList<CommerceLedgerEntry>? entries = null,
    long lastSequence = 0,
    long retentionFloorSequence = 0)
  {
    _entries = entries is null ? [] : new List<CommerceLedgerEntry>(entries);
    LastSequence = lastSequence;
    RetentionFloorSequence = retentionFloorSequence;
  }

  public long LastSequence;
  public long RetentionFloorSequence;

  public IReadOnlyList<CommerceLedgerEntry> Entries => _entries;

  public bool ContainsUnknownOutcome
  {
    get
    {
      foreach (CommerceLedgerEntry entry in _entries)
      {
        if (entry.IsUnknownOutcome)
        {
          return true;
        }
      }
      return false;
    }
  }
}
