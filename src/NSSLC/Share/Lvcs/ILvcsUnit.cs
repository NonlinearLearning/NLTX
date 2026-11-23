namespace Terraria.Lvcs;

/// <summary>
/// One raw content unit: a single file, or one entry produced by a source.
/// </summary>
/// <remarks>
/// The payload is bytes, not text, so a source may hold JSON, YAML or a binary table without
/// the contract knowing which. <see cref="Location" /> exists only for diagnostics.
/// </remarks>
public interface ILvcsUnit
{
  /// <summary>Human readable origin of this unit, used only in diagnostics.</summary>
  string Location { get; }

  /// <summary>Raw bytes handed to the codec.</summary>
  ReadOnlyMemory<byte> Payload { get; }
}
