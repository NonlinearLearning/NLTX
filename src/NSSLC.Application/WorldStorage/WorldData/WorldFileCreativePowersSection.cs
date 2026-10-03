using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Bounded CreativePowers world payload retained for a CreativePowers owner to decode.
/// </summary>
/// <remarks>
/// Power records are supplied by a runtime registry and can contain extension data. The
/// application layer therefore retains their exact bytes without depending on that registry.
/// </remarks>
public sealed class WorldFileCreativePowersSection
{
  public const string SectionId = "world.creative-powers";

  public WorldFileCreativePowersSection(ReadOnlyMemory<byte> serializedPayload)
  {
    SerializedPayload = serializedPayload.ToArray();
  }

  public ReadOnlyMemory<byte> SerializedPayload { get; }

  /// <summary>
  /// Gets the valid empty CreativePowers framing used by the current WorldFile format.
  /// </summary>
  /// <remarks>
  /// The legacy writer always emits a false record terminator, even when no persistent power
  /// is present. An empty byte sequence is therefore not a valid section payload.
  /// </remarks>
  public static WorldFileCreativePowersSection Empty => new(new byte[] { 0 });
}
