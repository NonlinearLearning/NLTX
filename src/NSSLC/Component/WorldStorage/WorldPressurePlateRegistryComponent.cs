using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.WorldStorage;

public sealed class WorldPressurePlateRegistryComponent : IDisposable
{
  private const int MaximumPlayerSlotCount = 255;
  private HashSet<TileCoordinate> _anchorSet = new();
  private readonly Dictionary<TileCoordinate, HashSet<int>> _pressedPlayersByAnchor = new();
  private readonly Dictionary<int, HashSet<TileCoordinate>> _pressedAnchorsByPlayer = new();
  private bool _isDisposed;

  private IReadOnlyList<TileCoordinate> _anchors = Array.Empty<TileCoordinate>();

  public IReadOnlyList<TileCoordinate> Anchors
  {
    get
    {
      VerifyAccess();
      return _anchors;
    }
  }

  public void Replace(IReadOnlyList<TileCoordinate> anchors)
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(anchors);
    TileCoordinate[] copiedAnchors = anchors.ToArray();
    if (copiedAnchors.Distinct().Count() != copiedAnchors.Length)
    {
      throw new ArgumentException("Pressure plate anchors must be unique.", nameof(anchors));
    }

    _anchors = Array.AsReadOnly(copiedAnchors);
    _anchorSet = copiedAnchors.ToHashSet();
    _pressedPlayersByAnchor.Clear();
    _pressedAnchorsByPlayer.Clear();
  }

  public bool ContainsAnchor(TileCoordinate anchor)
  {
    VerifyAccess();
    return _anchorSet.Contains(anchor);
  }

  public IReadOnlyList<TileCoordinate> CapturePressedAnchors(int playerSlot)
  {
    VerifyAccess();
    ValidatePlayerSlot(playerSlot);
    if (!_pressedAnchorsByPlayer.TryGetValue(playerSlot, out HashSet<TileCoordinate>? anchors))
    {
      return Array.Empty<TileCoordinate>();
    }

    TileCoordinate[] snapshot = anchors
      .OrderBy(static anchor => anchor.Y)
      .ThenBy(static anchor => anchor.X)
      .ToArray();
    return Array.AsReadOnly(snapshot);
  }

  public bool CommitPressedState(TileCoordinate anchor, int playerSlot, bool isPressed)
  {
    VerifyAccess();
    ValidatePlayerSlot(playerSlot);
    if (!_anchorSet.Contains(anchor))
    {
      throw new ArgumentException("The pressure plate anchor is not registered.", nameof(anchor));
    }

    if (isPressed)
    {
      if (!_pressedPlayersByAnchor.TryGetValue(anchor, out HashSet<int>? pressedPlayers))
      {
        pressedPlayers = new HashSet<int>();
        _pressedPlayersByAnchor.Add(anchor, pressedPlayers);
      }
      if (!_pressedAnchorsByPlayer.TryGetValue(playerSlot, out HashSet<TileCoordinate>? anchors))
      {
        anchors = new HashSet<TileCoordinate>();
        _pressedAnchorsByPlayer.Add(playerSlot, anchors);
      }

      bool addedByAnchor = pressedPlayers.Add(playerSlot);
      bool addedByPlayer = anchors.Add(anchor);
      if (addedByAnchor != addedByPlayer)
      {
        throw new InvalidOperationException("Pressure plate player state indexes are inconsistent.");
      }

      return addedByAnchor;
    }

    bool removedByAnchor = false;
    if (_pressedPlayersByAnchor.TryGetValue(anchor, out HashSet<int>? existingPlayers))
    {
      removedByAnchor = existingPlayers.Remove(playerSlot);
      if (existingPlayers.Count == 0)
      {
        _pressedPlayersByAnchor.Remove(anchor);
      }
    }

    bool removedByPlayer = false;
    if (_pressedAnchorsByPlayer.TryGetValue(playerSlot, out HashSet<TileCoordinate>? existingAnchors))
    {
      removedByPlayer = existingAnchors.Remove(anchor);
      if (existingAnchors.Count == 0)
      {
        _pressedAnchorsByPlayer.Remove(playerSlot);
      }
    }

    if (removedByAnchor != removedByPlayer)
    {
      throw new InvalidOperationException("Pressure plate player state indexes are inconsistent.");
    }

    return removedByAnchor;
  }

  public bool[] CreateLegacyPlayerStateSnapshot(TileCoordinate anchor)
  {
    VerifyAccess();
    if (!_anchorSet.Contains(anchor))
    {
      throw new ArgumentException("The pressure plate anchor is not registered.", nameof(anchor));
    }

    var snapshot = new bool[MaximumPlayerSlotCount];
    if (_pressedPlayersByAnchor.TryGetValue(anchor, out HashSet<int>? pressedPlayers))
    {
      foreach (int playerSlot in pressedPlayers)
      {
        snapshot[playerSlot] = true;
      }
    }

    return snapshot;
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _anchorSet.Clear();
    _pressedPlayersByAnchor.Clear();
    _pressedAnchorsByPlayer.Clear();
    _anchors = Array.Empty<TileCoordinate>();
    _isDisposed = true;
  }

  private void VerifyAccess()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }

  private static void ValidatePlayerSlot(int playerSlot)
  {
    if ((uint)playerSlot >= MaximumPlayerSlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }
  }
}
