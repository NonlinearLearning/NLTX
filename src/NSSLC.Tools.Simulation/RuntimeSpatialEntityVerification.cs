using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Player;
using Terraria.Relationships;
using Terraria.SpatialSimulation.Components;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class RuntimeSpatialEntityVerification
{
  internal static void AssertLiveParity(
    EntityRuntime entityRuntime,
    IReadOnlyList<RuntimeNpcEntity> npcs,
    IReadOnlyList<RuntimePlayerEntity> players,
    IReadOnlyCollection<int> requiredNpcNetIds,
    int duplicateNpcNetId,
    int expectedPlayerCount)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(requiredNpcNetIds);

    if (requiredNpcNetIds.Count != 8)
    {
      throw new InvalidOperationException(
        "The spatial host probe must require all eight supported NPC NetIds.");
    }

    if (expectedPlayerCount is < 0 or > 2 || players.Count != expectedPlayerCount)
    {
      throw new InvalidOperationException(
        $"Expected {expectedPlayerCount} runtime Players but received {players.Count}.");
    }

    var requiredNpcSet = new HashSet<int>();
    foreach (int requiredNetId in requiredNpcNetIds)
    {
      if (!requiredNpcSet.Add(requiredNetId))
      {
        throw new InvalidOperationException("The required supported NPC NetIds must be unique.");
      }

      if (!npcs.Any(npc => npc.Definition.NetId == requiredNetId))
      {
        throw new InvalidOperationException(
          $"The spatial host probe did not hydrate supported NPC NetId {requiredNetId}.");
      }
    }

    if (npcs.Count(npc => npc.Definition.NetId == duplicateNpcNetId) < 2)
    {
      throw new InvalidOperationException(
        $"The spatial host probe requires two independent NPC instances of NetId " +
        $"{duplicateNpcNetId}.");
    }

    var handles = new HashSet<RuntimeEntityHandle>();
    foreach (RuntimeNpcEntity npc in npcs)
    {
      if (!npc.RuntimeHandle.RuntimeId.Equals(entityRuntime.RuntimeId))
      {
        throw new InvalidOperationException("An NPC is attached to a different world runtime.");
      }

      if (!handles.Add(npc.RuntimeHandle))
      {
        throw new InvalidOperationException("Two spatial NPC entries share one runtime handle.");
      }

      AssertNpcParity(entityRuntime, npc);
    }

    foreach (RuntimePlayerEntity player in players)
    {
      if (!player.RuntimeHandle.RuntimeId.Equals(entityRuntime.RuntimeId))
      {
        throw new InvalidOperationException("A Player is attached to a different world runtime.");
      }

      if (!handles.Add(player.RuntimeHandle))
      {
        throw new InvalidOperationException("Two spatial entity entries share one runtime handle.");
      }

      AssertPlayerParity(entityRuntime, player);
    }
  }

  internal static void AssertInstanceIsolation(
    EntityRuntime entityRuntime,
    IReadOnlyList<RuntimeNpcEntity> npcs,
    IReadOnlyList<RuntimePlayerEntity> players,
    int duplicateNpcNetId,
    int expectedPlayerCount)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    if (expectedPlayerCount != players.Count)
    {
      throw new InvalidOperationException(
        "The Player isolation probe received the wrong Player count.");
    }

    RuntimeNpcEntity firstDuplicate = npcs.First(npc => npc.Definition.NetId == duplicateNpcNetId);
    RuntimeNpcEntity secondDuplicate = npcs.First(
      npc => npc.Definition.NetId == duplicateNpcNetId &&
        !npc.RuntimeHandle.Equals(firstDuplicate.RuntimeHandle));
    AssertNpcMutationIsolation(entityRuntime, firstDuplicate, secondDuplicate);
    if (players.Count == 2)
    {
      AssertPlayerMutationIsolation(entityRuntime, players[0], players[1]);
    }
  }

  internal static void AssertMovementWriteProtocol(
    EntityRuntime entityRuntime,
    RuntimeNpcEntity npc)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(npc);
    AssertMovementWriteProtocol(
      entityRuntime,
      npc.RuntimeHandle,
      () => npc.Movement,
      movement => npc.Movement = movement);
  }

  internal static void AssertMovementWriteProtocol(
    EntityRuntime entityRuntime,
    RuntimePlayerEntity player)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(player);
    AssertMovementWriteProtocol(
      entityRuntime,
      player.RuntimeHandle,
      () => player.Movement,
      movement => player.Movement = movement);
  }

  internal static void AssertPlayerDeathSpatialState(
    EntityRuntime entityRuntime,
    RuntimePlayerEntity player,
    EntityReference rootBeforeContact,
    Vector2 positionBeforeContact,
    ColliderComponent colliderBeforeContact)
  {
    AssertPlayerParity(entityRuntime, player);
    MovementStateComponent movement = player.Movement;
    if (!player.Reference.Equals(rootBeforeContact) ||
        !player.Lifecycle.IsDead ||
        movement.Position != positionBeforeContact ||
        movement.Velocity != Vector2.Zero ||
        player.Collider != colliderBeforeContact)
    {
      throw new InvalidOperationException(
        "NPC contact death must keep the Player root, location, and collider " +
        "while zeroing velocity.");
    }
  }

  internal static void AssertPlayerRespawn(
    EntityRuntime entityRuntime,
    RuntimePlayerEntity player,
    EntityReference rootBeforeDeath,
    ColliderComponent colliderBeforeDeath)
  {
    AssertPlayerParity(entityRuntime, player);
    MovementStateComponent movement = player.Movement;
    if (!player.Reference.Equals(rootBeforeDeath) ||
        player.Lifecycle.IsDead ||
        movement.Position != player.SpawnPosition ||
        movement.Velocity != Vector2.Zero ||
        movement.IsGrounded ||
        player.Collider != colliderBeforeDeath)
    {
      throw new InvalidOperationException(
        "Player respawn must preserve the entity root and collider and commit the spawn location.");
    }
  }

  internal static void AssertContactImmunityAndRespawn(
    EntityRuntime entityRuntime,
    RuntimePlayerEntity player,
    RuntimeNpcEntity contactNpc,
    long tickNumber)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(contactNpc);
    if (!contactNpc.IsActive || !contactNpc.Definition.Capabilities.Hostile)
    {
      throw new InvalidOperationException(
        "The Player contact probe requires an active hostile NPC.");
    }

    if (!entityRuntime.TryEdit<PlayerVitalState>(
          player.RuntimeHandle,
          (ref PlayerVitalState vitals) => vitals.Life = vitals.EffectiveLifeMaximum))
    {
      throw new InvalidOperationException(
        "The Player contact probe could not restore test vitals.");
    }

    RuntimeNpcContactSnapshot contact = contactNpc.CaptureContactSnapshot();
    EntityReference rootBeforeContact = player.Reference;
    ColliderComponent colliderBeforeContact = player.Collider;
    Vector2 positionBeforeContact = player.Movement.Position;
    RuntimeNpcContactSnapshot nonlethalContact = contact with { Damage = 1 };
    int initialLife = player.Vitals.Life;
    if (!player.TryTakeNpcContactDamage(nonlethalContact, tickNumber))
    {
      throw new InvalidOperationException("The first NPC contact should apply nonlethal damage.");
    }

    int lifeAfterContact = player.Vitals.Life;
    if (lifeAfterContact <= 0 ||
        lifeAfterContact >= initialLife ||
        player.TryTakeNpcContactDamage(nonlethalContact, tickNumber + 1) ||
        player.Vitals.Life != lifeAfterContact)
    {
      throw new InvalidOperationException(
        "Player contact immunity must reject repeated same-window damage.");
    }

    for (int tick = 0; tick < 40; tick++)
    {
      player.AdvanceLifecycle();
    }

    RuntimeNpcContactSnapshot lethalContact = contact with { Damage = 10000 };
    if (!player.TryTakeNpcContactDamage(lethalContact, tickNumber + 41))
    {
      throw new InvalidOperationException(
        "NPC contact should commit the lethal Player transition.");
    }

    AssertPlayerDeathSpatialState(
      entityRuntime,
      player,
      rootBeforeContact,
      positionBeforeContact,
      colliderBeforeContact);

    for (int tick = 0; tick <= 600 && player.Lifecycle.IsDead; tick++)
    {
      player.AdvanceLifecycle();
    }

    AssertPlayerRespawn(entityRuntime, player, rootBeforeContact, colliderBeforeContact);
  }

  internal static void AssertJumpAndLandingObserved(RuntimePlayerEntity player)
  {
    ArgumentNullException.ThrowIfNull(player);
    if (player.JumpCount <= 0 || player.LandingCount <= 0)
    {
      throw new InvalidOperationException(
        "The Player movement probe must observe both a jump and a landing.");
    }
  }

  private static void AssertNpcParity(EntityRuntime entityRuntime, RuntimeNpcEntity npc)
  {
    LocationComponent location = Capture<LocationComponent>(
      entityRuntime,
      npc.RuntimeHandle,
      "NPC location");
    VelocityComponent velocity = Capture<VelocityComponent>(
      entityRuntime,
      npc.RuntimeHandle,
      "NPC velocity");
    ColliderComponent collider = Capture<ColliderComponent>(
      entityRuntime,
      npc.RuntimeHandle,
      "NPC collider");
    MovementGroundedStateComponent grounded = Capture<MovementGroundedStateComponent>(
      entityRuntime,
      npc.RuntimeHandle,
      "NPC grounded state");
    if (entityRuntime.Has<MovementStateComponent>(npc.RuntimeHandle))
    {
      throw new InvalidOperationException(
        "MovementStateComponent must remain an invocation value only.");
    }

    AssertSpatialCopies(
      entityRuntime,
      npc.RuntimeHandle,
      location,
      velocity,
      collider,
      grounded);
    if (!SameLocation(npc.Location, location) ||
        !SameVelocity(npc.Velocity, velocity) ||
        npc.Collider != collider)
    {
      throw new InvalidOperationException(
        "The NPC owner view must use the attached spatial components.");
    }

    AssertMovementValue(npc.Movement, location, velocity, grounded);

    RuntimeNpcContactSnapshot contact = npc.CaptureContactSnapshot();
    Vector2 contactPosition = new(location.X + collider.OffsetX, location.Y + collider.OffsetY);
    if (contact.Hitbox.Position != contactPosition ||
        contact.Hitbox.Center != contactPosition +
          new Vector2(contact.Hitbox.Width * 0.5f, contact.Hitbox.Height * 0.5f) ||
        contact.Hitbox.Width != Math.Max(1, (int)collider.Width) ||
        contact.Hitbox.Height != Math.Max(1, (int)collider.Height))
    {
      throw new InvalidOperationException(
        "NPC contact geometry must come from the attached collider.");
    }
  }

  private static void AssertPlayerParity(EntityRuntime entityRuntime, RuntimePlayerEntity player)
  {
    LocationComponent location = Capture<LocationComponent>(
      entityRuntime,
      player.RuntimeHandle,
      "Player location");
    VelocityComponent velocity = Capture<VelocityComponent>(
      entityRuntime,
      player.RuntimeHandle,
      "Player velocity");
    ColliderComponent collider = Capture<ColliderComponent>(
      entityRuntime,
      player.RuntimeHandle,
      "Player collider");
    MovementGroundedStateComponent grounded = Capture<MovementGroundedStateComponent>(
      entityRuntime,
      player.RuntimeHandle,
      "Player grounded state");
    if (entityRuntime.Has<MovementStateComponent>(player.RuntimeHandle))
    {
      throw new InvalidOperationException(
        "MovementStateComponent must remain an invocation value only.");
    }

    AssertSpatialCopies(
      entityRuntime,
      player.RuntimeHandle,
      location,
      velocity,
      collider,
      grounded);
    if (!SameLocation(player.Location, location) ||
        !SameVelocity(player.Velocity, velocity) ||
        player.Collider != collider)
    {
      throw new InvalidOperationException(
        "The Player owner view must use the attached spatial components.");
    }

    AssertMovementValue(player.Movement, location, velocity, grounded);
  }

  private static void AssertSpatialCopies(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    LocationComponent location,
    VelocityComponent velocity,
    ColliderComponent collider,
    MovementGroundedStateComponent grounded)
  {
    AssertQueryMembership<LocationComponent>(entityRuntime, handle, "location");
    AssertQueryMembership<VelocityComponent>(entityRuntime, handle, "velocity");
    AssertQueryMembership<ColliderComponent>(entityRuntime, handle, "collider");
    AssertQueryMembership<MovementGroundedStateComponent>(entityRuntime, handle, "grounded state");
    LocationComponent locationCopy = location;
    VelocityComponent velocityCopy = velocity;
    locationCopy.X = location.X == 123456.75f ? 123456.5f : 123456.75f;
    velocityCopy.Y = velocity.Y == 123456.75f ? 123456.5f : 123456.75f;
    if (locationCopy.X == location.X || velocityCopy.Y == velocity.Y)
    {
      throw new InvalidOperationException(
        "The copied spatial values did not change in the alias probe.");
    }

    if (!TryCapture(entityRuntime, handle, out LocationComponent currentLocation) ||
        !TryCapture(entityRuntime, handle, out VelocityComponent currentVelocity) ||
        !TryCapture(entityRuntime, handle, out ColliderComponent currentCollider) ||
        !TryCapture(entityRuntime, handle, out MovementGroundedStateComponent currentGrounded) ||
        !SameLocation(currentLocation, location) ||
        !SameVelocity(currentVelocity, velocity) ||
        currentCollider != collider ||
        currentGrounded != grounded)
    {
      throw new InvalidOperationException(
        "Captured spatial values must not expose mutable component storage.");
    }
  }

  private static void AssertMovementValue(
    MovementStateComponent movement,
    LocationComponent location,
    VelocityComponent velocity,
    MovementGroundedStateComponent grounded)
  {
    if (movement.Position != new Vector2(location.X, location.Y) ||
        movement.Velocity != new Vector2(velocity.X, velocity.Y) ||
        movement.IsGrounded != grounded.IsGrounded)
    {
      throw new InvalidOperationException(
        "The movement algorithm value must be projected from the current entity components.");
    }
  }

  private static void AssertSameMovement(
    MovementStateComponent actual,
    MovementStateComponent expected)
  {
    if (actual.Position != expected.Position ||
        actual.Velocity != expected.Velocity ||
        actual.IsGrounded != expected.IsGrounded)
    {
      throw new InvalidOperationException(
        "A spatial write to one entity changed a separate entity.");
    }
  }

  private static void AssertNpcMutationIsolation(
    EntityRuntime entityRuntime,
    RuntimeNpcEntity first,
    RuntimeNpcEntity second)
  {
    MovementStateComponent firstBefore = first.Movement;
    MovementStateComponent secondBefore = second.Movement;
    try
    {
      first.Movement = new MovementStateComponent(
        firstBefore.Position + new Vector2(3.0f, 5.0f),
        firstBefore.Velocity + new Vector2(1.0f, -2.0f),
        isGrounded: !firstBefore.IsGrounded);
      AssertNpcParity(entityRuntime, first);
      AssertNpcParity(entityRuntime, second);
      AssertSameMovement(second.Movement, secondBefore);
    }
    finally
    {
      first.Movement = firstBefore;
    }
  }

  private static void AssertPlayerMutationIsolation(
    EntityRuntime entityRuntime,
    RuntimePlayerEntity first,
    RuntimePlayerEntity second)
  {
    MovementStateComponent firstBefore = first.Movement;
    MovementStateComponent secondBefore = second.Movement;
    try
    {
      first.Movement = new MovementStateComponent(
        firstBefore.Position + new Vector2(3.0f, 5.0f),
        firstBefore.Velocity + new Vector2(1.0f, -2.0f),
        isGrounded: !firstBefore.IsGrounded);
      AssertPlayerParity(entityRuntime, first);
      AssertPlayerParity(entityRuntime, second);
      AssertSameMovement(second.Movement, secondBefore);
    }
    finally
    {
      first.Movement = firstBefore;
    }
  }

  private static void AssertMovementWriteProtocol(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    Func<MovementStateComponent> readMovement,
    Action<MovementStateComponent> writeMovement)
  {
    MovementStateComponent original = readMovement();
    MovementStateComponent uncommittedCopy = original;
    uncommittedCopy.Position += new Vector2(13.0f, 17.0f);
    uncommittedCopy.Velocity += new Vector2(2.0f, 4.0f);
    if (uncommittedCopy.Position == original.Position ||
        uncommittedCopy.Velocity == original.Velocity)
    {
      throw new InvalidOperationException(
        "The local movement copy did not change in the commit probe.");
    }

    AssertSameMovement(readMovement(), original);

    EntityComponentSnapshot<LocationComponent> beforeLocation = CaptureVersioned<LocationComponent>(
      entityRuntime,
      handle,
      "location before movement commit");
    EntityComponentSnapshot<VelocityComponent> beforeVelocity = CaptureVersioned<VelocityComponent>(
      entityRuntime,
      handle,
      "velocity before movement commit");
    EntityComponentSnapshot<MovementGroundedStateComponent> beforeGrounded =
      CaptureVersioned<MovementGroundedStateComponent>(
        entityRuntime,
        handle,
        "grounded state before movement commit");
    EntityComponentSnapshot<ColliderComponent> beforeCollider = CaptureVersioned<ColliderComponent>(
      entityRuntime,
      handle,
      "collider before movement commit");
    var committed = new MovementStateComponent(
      original.Position + new Vector2(3.0f, 5.0f),
      original.Velocity + new Vector2(1.0f, -2.0f),
      isGrounded: !original.IsGrounded);
    try
    {
      writeMovement(committed);
      AssertSameMovement(readMovement(), committed);

      EntityComponentSnapshot<LocationComponent> afterLocation =
        CaptureVersioned<LocationComponent>(
          entityRuntime,
          handle,
          "location after movement commit");
      EntityComponentSnapshot<VelocityComponent> afterVelocity =
        CaptureVersioned<VelocityComponent>(
          entityRuntime,
          handle,
          "velocity after movement commit");
      EntityComponentSnapshot<MovementGroundedStateComponent> afterGrounded =
        CaptureVersioned<MovementGroundedStateComponent>(
          entityRuntime,
          handle,
          "grounded state after movement commit");
      EntityComponentSnapshot<ColliderComponent> afterCollider =
        CaptureVersioned<ColliderComponent>(
          entityRuntime,
          handle,
          "collider after movement commit");
      if (afterLocation.AttachmentRevision != beforeLocation.AttachmentRevision ||
          afterVelocity.AttachmentRevision != beforeVelocity.AttachmentRevision ||
          afterGrounded.AttachmentRevision != beforeGrounded.AttachmentRevision ||
          afterCollider.AttachmentRevision != beforeCollider.AttachmentRevision ||
          afterLocation.DataRevision <= beforeLocation.DataRevision ||
          afterVelocity.DataRevision <= beforeVelocity.DataRevision ||
          afterGrounded.DataRevision <= beforeGrounded.DataRevision ||
          afterCollider.DataRevision != beforeCollider.DataRevision)
      {
        throw new InvalidOperationException(
          "Movement commit must advance spatial data revisions without replacing " +
          "attachments or editing geometry.");
      }
    }
    finally
    {
      writeMovement(original);
    }
  }

  private static TComponent Capture<TComponent>(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    string description)
    where TComponent : struct
  {
    if (!TryCapture(entityRuntime, handle, out TComponent component))
    {
      throw new InvalidOperationException($"The {description} component could not be captured.");
    }

    return component;
  }

  private static void AssertQueryMembership<TComponent>(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    string description)
    where TComponent : notnull
  {
    if (!entityRuntime.Match<TComponent>().Contains(handle))
    {
      throw new InvalidOperationException(
        $"The runtime query for {description} must include the owning entity handle.");
    }
  }

  private static EntityComponentSnapshot<TComponent> CaptureVersioned<TComponent>(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    string description)
    where TComponent : struct
  {
    if (!entityRuntime.TryCaptureVersioned<TComponent, TComponent>(
          handle,
          static component => component,
          out EntityComponentSnapshot<TComponent> snapshot))
    {
      throw new InvalidOperationException($"The {description} component could not be captured.");
    }

    return snapshot;
  }

  private static bool TryCapture<TComponent>(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    out TComponent component)
    where TComponent : struct
  {
    return entityRuntime.TryCapture<TComponent, TComponent>(
      handle,
      static value => value,
      out component);
  }

  private static bool SameLocation(LocationComponent first, LocationComponent second)
  {
    return first.X == second.X && first.Y == second.Y;
  }

  private static bool SameVelocity(VelocityComponent first, VelocityComponent second)
  {
    return first.X == second.X && first.Y == second.Y;
  }
}
