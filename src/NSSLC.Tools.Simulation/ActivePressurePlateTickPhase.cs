using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Simulation;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class ActivePressurePlateTickPhase
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort PressurePlateTileType = 135;
  private const ushort WeightedPressurePlateTileType = 428;
  private readonly RuntimePlayerStore _players;
  private readonly WiredActuatorTrigger _wiringActuatorTrigger = new();

  public ActivePressurePlateTickPhase(RuntimePlayerStore players)
  {
    _players = players ?? throw new ArgumentNullException(nameof(players));
  }

  public int ActivationCount { get; private set; }

  public int ActuatorToggleCount { get; private set; }

  public IReadOnlyList<ushort> RecognizedUnsupportedWiredDeviceTileTypes =>
    _wiringActuatorTrigger.RecognizedUnsupportedWiredDeviceTileTypes;

  public void Execute(WorldSimulationTickContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    WorldPressurePlateRegistryComponent registry = context.Session.Storage.PressurePlates;
    if (registry.Anchors.Count == 0)
    {
      return;
    }

    var changedAnchors = new HashSet<TileCoordinate>();
    var activatedAnchors = new List<TileCoordinate>();
    foreach (RuntimePlayerEntity player in _players.Players)
    {
      HashSet<TileCoordinate> currentAnchors = CapturePressedAnchors(
        context.Session,
        registry,
        player);
      IReadOnlyList<TileCoordinate> previousAnchors =
        registry.CapturePressedAnchors(player.Slot);

      foreach (TileCoordinate anchor in currentAnchors)
      {
        if (registry.CommitPressedState(anchor, player.Slot, isPressed: true))
        {
          changedAnchors.Add(anchor);
          activatedAnchors.Add(anchor);
          ActivationCount++;
        }
      }

      foreach (TileCoordinate anchor in previousAnchors)
      {
        if (!currentAnchors.Contains(anchor) &&
            registry.CommitPressedState(anchor, player.Slot, isPressed: false))
        {
          changedAnchors.Add(anchor);
        }
      }
    }

    if (changedAnchors.Count == 0)
    {
      return;
    }

    WorldStorageOperationResult projection =
      LegacyWorldPressurePlateProjection.PublishCommittedPressStates(
        context.Session,
        changedAnchors);
    if (!projection.Succeeded)
    {
      throw new InvalidOperationException(
        $"Committed pressure-plate state could not be projected: " +
        $"{projection.Failure.Kind} {projection.Failure.Detail}");
    }

    foreach (TileCoordinate anchor in activatedAnchors)
    {
      ActuatorToggleCount += _wiringActuatorTrigger.Trigger(context.Session, anchor);
    }
  }

  private static HashSet<TileCoordinate> CapturePressedAnchors(
    LoadedWorldSession session,
    WorldPressurePlateRegistryComponent registry,
    RuntimePlayerEntity player)
  {
    var pressedAnchors = new HashSet<TileCoordinate>();
    if (player.Lifecycle.IsDead)
    {
      return pressedAnchors;
    }

    TileMapStore tileMap = session.Storage.TileMap;
    if (tileMap.Width <= 0 || tileMap.Height <= 0)
    {
      return pressedAnchors;
    }

    Vector2 position = player.Movement.Position;
    int firstX = Math.Clamp((int)MathF.Floor(position.X / 16f), 0, tileMap.Width - 1);
    int lastX = Math.Clamp(
      (int)MathF.Floor((position.X + RuntimePlayerStore.PlayerWidth) / 16f),
      0,
      tileMap.Width - 1);
    int firstY = Math.Clamp((int)MathF.Floor(position.Y / 16f), 0, tileMap.Height - 1);
    int lastY = Math.Clamp(
      (int)MathF.Floor((position.Y + RuntimePlayerStore.PlayerHeight) / 16f),
      0,
      tileMap.Height - 1);

    for (int tileX = firstX; tileX <= lastX; tileX++)
    {
      for (int tileY = firstY; tileY <= lastY; tileY++)
      {
        var anchor = new TileCoordinate(tileX, tileY);
        if (!registry.ContainsAnchor(anchor))
        {
          continue;
        }

        TileCellState tile = tileMap.GetTile(tileX, tileY);
        if ((tile.TileHeader & ActiveTileFlag) == 0 ||
            tile.Type is not (PressurePlateTileType or WeightedPressurePlateTileType) ||
            !OverlapsTile(position, tileX, tileY))
        {
          continue;
        }

        pressedAnchors.Add(anchor);
      }
    }

    return pressedAnchors;
  }

  private static bool OverlapsTile(Vector2 position, int tileX, int tileY)
  {
    float tileLeft = tileX * 16f;
    float tileTop = tileY * 16f;
    return position.X < tileLeft + 16f &&
      position.X + RuntimePlayerStore.PlayerWidth > tileLeft &&
      position.Y < tileTop + 16f &&
      position.Y + RuntimePlayerStore.PlayerHeight > tileTop;
  }
}
