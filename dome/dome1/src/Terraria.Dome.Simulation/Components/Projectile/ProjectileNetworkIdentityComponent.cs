using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileNetworkIdentityComponent(
  PlayerHandle Owner,
  int Identity,
  Guid? ProjectileUuid);
