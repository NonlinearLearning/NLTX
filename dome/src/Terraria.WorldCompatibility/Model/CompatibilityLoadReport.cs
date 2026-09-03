using System;
using System.Collections.Generic;

namespace Terraria.WorldCompatibility.Model;

public sealed class CompatibilityLoadReport
{
  public CompatibilityLoadReport(IReadOnlyList<CompatibilityUnsupportedRecord> unsupportedRecords)
  {
    UnsupportedRecords = new CompatibilityReadOnlyList<CompatibilityUnsupportedRecord>(
      unsupportedRecords ?? throw new ArgumentNullException(nameof(unsupportedRecords)));
  }

  public bool HasUnsupportedRecords => UnsupportedRecords.Count != 0;

  public IReadOnlyList<CompatibilityUnsupportedRecord> UnsupportedRecords { get; }
}
