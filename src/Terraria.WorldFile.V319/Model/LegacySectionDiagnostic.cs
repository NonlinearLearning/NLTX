using System;
using System.Collections.Generic;

namespace Terraria.WorldFile.V319.Model;

public sealed class LegacySectionDiagnostic
{
  public LegacySectionDiagnostic(
    int sectionIndex,
    long offset,
    string message,
    IReadOnlyList<byte>? payload = null)
  {
    SectionIndex = sectionIndex;
    Offset = offset;
    Message = message ?? throw new ArgumentNullException(nameof(message));
    Payload = new LegacyReadOnlyList<byte>(payload ?? []);
  }

  public string Message { get; }

  public long Offset { get; }

  public IReadOnlyList<byte> Payload { get; }

  public int SectionIndex { get; }
}
