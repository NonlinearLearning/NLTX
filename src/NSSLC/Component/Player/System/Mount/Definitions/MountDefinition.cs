namespace Terraria.Player.Mount;

public sealed class MountDefinition
{
  public MountDefinition(
    int id,
    int buffType,
    MountGeometryDefinition geometry,
    MountAnimationDefinition animation,
    MountMovementDefinition movement,
    MountAbilityDefinition ability,
    MountPresentationDefinition presentation)
  {
    if (id < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    ArgumentNullException.ThrowIfNull(geometry);
    animation.Validate();
    movement.Validate();
    ability.Validate();

    Id = id;
    BuffType = buffType;
    Geometry = geometry;
    Animation = animation;
    Movement = movement;
    Ability = ability;
    Presentation = presentation;
  }

  public int Id { get; }

  public int BuffType { get; }

  public MountGeometryDefinition Geometry { get; }

  public MountAnimationDefinition Animation { get; }

  public MountMovementDefinition Movement { get; }

  public MountAbilityDefinition Ability { get; }

  public MountPresentationDefinition Presentation { get; }
}
