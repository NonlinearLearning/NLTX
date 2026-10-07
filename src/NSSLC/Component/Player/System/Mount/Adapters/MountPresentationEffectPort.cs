using System.Numerics;

namespace Terraria.Player.Mount;

public enum MountPresentationEffectKind
{
  Light = 0,
  Dust = 1,
  Sound = 2,
}

public readonly record struct MountPresentationEffectIntent(
  MountPresentationEffectKind Kind,
  Vector2 Position,
  Vector3 LightColor,
  int DustType,
  bool DustNoGravity,
  int SoundId)
{
  public bool IsValid => Kind switch
  {
    MountPresentationEffectKind.Light =>
      LightColor.X >= 0f && LightColor.Y >= 0f && LightColor.Z >= 0f,
    MountPresentationEffectKind.Dust => DustType >= 0,
    MountPresentationEffectKind.Sound => SoundId >= 0,
    _ => false,
  };
}

public interface IMountPresentationEffectPort
{
  MountAdapterCommitResult Publish(in MountPresentationEffectIntent intent);
}
