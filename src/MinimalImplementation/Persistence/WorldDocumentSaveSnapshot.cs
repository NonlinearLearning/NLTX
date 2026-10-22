using System;
using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>A detached persistence document captured for one world scope and commit version.</summary>
public sealed class WorldDocumentSaveSnapshot
{
  public WorldDocumentSaveSnapshot(
    long worldScope,
    CommitVersion capturedVersion,
    WorldPersistenceDocument document)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    if (!capturedVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(capturedVersion));
    }

    ArgumentNullException.ThrowIfNull(document);
    WorldScope = worldScope;
    CapturedVersion = capturedVersion;
    Document = document;
  }

  public long WorldScope { get; }

  public CommitVersion CapturedVersion { get; }

  public WorldPersistenceDocument Document { get; }
}
