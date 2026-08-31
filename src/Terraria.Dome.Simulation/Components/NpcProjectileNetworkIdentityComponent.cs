using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct NpcProjectileNetworkIdentityComponent(
  NpcHandle Owner,
  int Identity,
  Guid? ProjectileUuid);
