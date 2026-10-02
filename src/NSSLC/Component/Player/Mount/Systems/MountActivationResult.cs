namespace Terraria.Player.Mount;

public enum MountActivationResultKind
{
  Activated = 0,
  AlreadyActive = 1,
  UnknownDefinition = 2,
}

public readonly record struct MountActivationResult(
  MountActivationResultKind Kind,
  ContentId<MountDefinition>? MountType)
{
  public bool Changed => Kind == MountActivationResultKind.Activated;
}
