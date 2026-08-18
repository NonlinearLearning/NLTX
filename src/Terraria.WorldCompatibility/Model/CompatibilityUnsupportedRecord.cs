using System;
using System.Collections.Generic;

namespace Terraria.WorldCompatibility.Model;

public sealed class CompatibilityUnsupportedRecord
{
  public CompatibilityUnsupportedRecord(
    int sectionIndex,
    long offset,
    string reason,
    int? type = null,
    int? x = null,
    int? y = null,
    IReadOnlyList<byte>? payload = null,
    bool isRequired = false)
  {
    SectionIndex = sectionIndex;
    Offset = offset;
    Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    Type = type;
    X = x;
    Y = y;
    Payload = new CompatibilityReadOnlyList<byte>(payload ?? []);
    IsRequired = isRequired;
  }

  public long Offset { get; }

  public IReadOnlyList<byte> Payload { get; }

  public string Reason { get; }

  public int SectionIndex { get; }

  public int? Type { get; }

  public bool IsRequired { get; }

  public int? X { get; }

  public int? Y { get; }
}
