namespace Terraria.Content;

public sealed record BuffDefinition(
  int TypeId,
  string? PersistentId,
  bool IsDebuff,
  bool AffectsPvp,
  bool IsPersistent,
  bool NoSave)
{
  public BuffIdentityDefinition Identity => new(TypeId, PersistentId);

  public BuffRuleDefinition Rules => new(IsDebuff, AffectsPvp, IsPersistent, NoSave);

  public BuffPresentationDefinition Presentation { get; init; } = new();
}
