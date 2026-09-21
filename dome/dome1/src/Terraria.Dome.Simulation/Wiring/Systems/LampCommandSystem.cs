using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.Wiring.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class LampCommandSystem
{
  public bool TryCreateFrameCommands(
    WorldGrid world,
    LampDefinitionRegistry definitions,
    int hitX,
    int hitY,
    MechanismActivationKind activationKind,
    long firstSequence,
    out IReadOnlyList<TileFrameCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    commands = [];
    if (firstSequence < 0 || !world.Contains(hitX, hitY))
    {
      return false;
    }

    WorldTile tile = world.GetTile(hitX, hitY);
    if (!tile.IsActive || !definitions.TryGet(tile.Type, out _))
    {
      return false;
    }

    commands = CreateFrameCommands(
      world,
      definitions,
      hitX,
      hitY,
      activationKind,
      firstSequence);
    return true;
  }

  public IReadOnlyList<TileFrameCommand> CreateFrameCommands(
    WorldGrid world,
    LampDefinitionRegistry definitions,
    int hitX,
    int hitY,
    MechanismActivationKind activationKind,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    if (firstSequence < 0 || !world.Contains(hitX, hitY))
    {
      return [];
    }

    WorldTile hitTile = world.GetTile(hitX, hitY);
    if (!hitTile.IsActive || !definitions.TryGet(hitTile.Type, out LampDefinition definition))
    {
      return [];
    }

    int widthPixels = definition.Width * 18;
    int heightPixels = definition.Height * 18;
    int originX = hitX - Modulo(hitTile.FrameX, widthPixels) / 18;
    int originY = hitY - Modulo(hitTile.FrameY, heightPixels) / 18;
    if (!world.Contains(originX, originY) ||
        !world.Contains(originX + definition.Width - 1, originY + definition.Height - 1))
    {
      return [];
    }

    WorldTile originTile = world.GetTile(originX, originY);
    bool isLit = definition.FrameXOffset != 0 && originTile.FrameX >= definition.FrameXOffset ||
      definition.FrameYOffset != 0 && originTile.FrameY >= definition.FrameYOffset;
    bool targetIsLit = activationKind switch
    {
      MechanismActivationKind.Activate or MechanismActivationKind.Open => true,
      MechanismActivationKind.Close => false,
      MechanismActivationKind.Toggle => !isLit,
      _ => isLit
    };
    if (targetIsLit == isLit)
    {
      return [];
    }

    short deltaX = targetIsLit ? definition.FrameXOffset : (short)-definition.FrameXOffset;
    short deltaY = targetIsLit ? definition.FrameYOffset : (short)-definition.FrameYOffset;
    List<TileFrameCommand> commands = new(definition.Width * definition.Height);
    WiringSequenceAllocator sequenceAllocator = new(firstSequence);
    if (!sequenceAllocator.TryReserve(definition.Width * definition.Height, out long sequence))
    {
      return [];
    }

    for (int row = 0; row < definition.Height; row++)
    {
      for (int column = 0; column < definition.Width; column++)
      {
        int x = originX + column;
        int y = originY + row;
        WorldTile tile = world.GetTile(x, y);
        if (!tile.IsActive || tile.Type != definition.TileType)
        {
          return [];
        }

        commands.Add(new TileFrameCommand(
          sequence + commands.Count,
          x,
          y,
          checked((short)(tile.FrameX + deltaX)),
          checked((short)(tile.FrameY + deltaY))));
      }
    }

    return commands;
  }

  public IReadOnlyList<TileChangeCommand> CreateCommands(
    LampComponent lamp,
    MechanismActivationCommand activation,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(lamp);
    if (activation.MechanismId != lamp.MechanismId || firstSequence < 0)
    {
      return [];
    }

    bool isLit = activation.Kind switch
    {
      MechanismActivationKind.Activate or MechanismActivationKind.Open => true,
      MechanismActivationKind.Close => false,
      MechanismActivationKind.Toggle => !lamp.IsLit,
      _ => lamp.IsLit
    };
    WiringSequenceAllocator sequenceAllocator = new(firstSequence);
    if (!sequenceAllocator.TryReserve(1, out long sequence))
    {
      return [];
    }

    lamp.SetLit(isLit);
    WiringTileCoordinate tile = lamp.Tile;
    return [new TileChangeCommand(
      sequence,
      tile.X,
      tile.Y,
      TileChangeKind.Place,
      isLit ? lamp.LitTileType : lamp.UnlitTileType)];
  }

  private static int Modulo(int value, int divisor)
  {
    int remainder = value % divisor;
    return remainder < 0 ? remainder + divisor : remainder;
  }
}
