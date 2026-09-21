namespace Terraria.NonAuthoritative.Diagnostics;

public readonly record struct TimeLogEntryCommand(bool PendingDisplay)
{
  public void Apply(TimeLogEntryState entry)
  {
    ArgumentNullException.ThrowIfNull(entry);
    entry.SetPendingDisplay(PendingDisplay);
  }
}
