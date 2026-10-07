using System.Numerics;
using Terraria.Relationships;

namespace Terraria.Network;

/// <summary>Player actor facts captured from the same loaded world runtime.</summary>
public readonly record struct SocialPlayerNpcInteractionSnapshot(
  byte PlayerSlot,
  Vector2 Position,
  int Width,
  int Height,
  bool Active,
  bool Dead,
  bool ShouldNotDraw,
  float Stealth,
  EntityRuntimeId WorldRuntimeId);
