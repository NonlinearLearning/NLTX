using System;
using System.Collections.Generic;

using Terraria.WorldStorage;

internal static class ProjectileIdentityIndexVerification
{
  public static void Run()
  {
    var index = new ProjectileIdentityIndex();
    var identity = new OwnerProjectileIdentity(new PlayerSlot(255), 12);
    var firstHandle = new ProjectileHandle(new ProjectileSlot(4), 1);
    var replacementHandle = new ProjectileHandle(new ProjectileSlot(8), 2);
    var conflictHandle = new ProjectileHandle(new ProjectileSlot(9), 3);

    Assert(index.TryRegister(identity, firstHandle), "Initial identity should register.");
    Assert(
      index.TryRegister(identity, firstHandle),
      "Registering the same mapping should be idempotent.");
    AssertEqual(1, index.Count, "An idempotent registration must not add another mapping.");
    Assert(index.TryGetHandle(identity, out var foundHandle), "Owner identity should resolve.");
    AssertEqual(firstHandle, foundHandle, "Owner identity should resolve to its handle.");
    Assert(index.TryGetIdentity(firstHandle, out var foundIdentity), "Handle should resolve.");
    AssertEqual(identity, foundIdentity, "Handle should resolve to its owner identity.");

    Assert(
      !index.TryRegister(identity, conflictHandle),
      "A live owner identity must not be registered to a second handle.");
    var secondIdentity = new OwnerProjectileIdentity(new PlayerSlot(1), 20);
    Assert(
      !index.TryRegister(secondIdentity, firstHandle),
      "A handle must not be registered to a second owner identity.");
    AssertEqual(1, index.Count, "Conflicting registrations must leave the index unchanged.");

    Assert(
      index.TryReplace(identity, firstHandle, replacementHandle),
      "An expected handle may be atomically replaced.");
    Assert(
      !index.TryUnregister(firstHandle, out _),
      "A stale generation must not unregister a replacement.");
    Assert(
      !index.TryReplace(identity, firstHandle, conflictHandle),
      "A stale expected handle must not replace the current mapping.");
    Assert(index.TryGetHandle(identity, out foundHandle), "Replacement should remain indexed.");
    AssertEqual(replacementHandle, foundHandle, "Replacement handle should be current.");

    var changedIdentity = new OwnerProjectileIdentity(new PlayerSlot(2), 35);
    Assert(
      index.TryReplace(identity, replacementHandle, changedIdentity, conflictHandle),
      "A recycled projectile can atomically change owner identity and handle.");
    Assert(
      !index.TryGetHandle(identity, out _),
      "Identity replacement should remove the prior key.");
    Assert(
      index.TryGetHandle(changedIdentity, out foundHandle),
      "Identity replacement should add the new key.");
    AssertEqual(conflictHandle, foundHandle, "The new identity should resolve to the new handle.");
    Assert(!index.TryGetIdentity(replacementHandle, out _), "The old handle should become stale.");

    var snapshot = new[]
    {
      new KeyValuePair<OwnerProjectileIdentity, ProjectileHandle>(
        secondIdentity,
        conflictHandle),
    };
    index.Rebuild(snapshot);
    AssertEqual(1, index.Count, "Rebuild should replace the complete prior index.");
    Assert(
      !index.TryGetHandle(identity, out _),
      "Rebuild should remove entries absent from its snapshot.");
    Assert(
      index.TryGetIdentity(conflictHandle, out foundIdentity),
      "Rebuilt handle should resolve.");
    AssertEqual(secondIdentity, foundIdentity, "Rebuilt handle should resolve to its identity.");

    AssertThrows<ArgumentException>(
      () => index.Rebuild(new[]
      {
        new KeyValuePair<OwnerProjectileIdentity, ProjectileHandle>(
          identity,
          firstHandle),
        new KeyValuePair<OwnerProjectileIdentity, ProjectileHandle>(
          identity,
          replacementHandle),
      }),
      "A duplicate owner identity must reject the snapshot.");
    Assert(
      index.TryGetHandle(secondIdentity, out foundHandle),
      "A rejected rebuild must preserve the prior index.");
    AssertEqual(
      conflictHandle,
      foundHandle,
      "A rejected rebuild must not partially replace entries.");

    AssertThrows<ArgumentOutOfRangeException>(
      () => index.TryRegister(
        new OwnerProjectileIdentity(new PlayerSlot(-1), 0),
        firstHandle),
      "Invalid owner slots must be rejected.");
    AssertThrows<ArgumentOutOfRangeException>(
      () => index.TryRegister(
        new OwnerProjectileIdentity(new PlayerSlot(1), -1),
        firstHandle),
      "Negative identities must be rejected.");
    AssertThrows<ArgumentOutOfRangeException>(
      () => index.TryRegister(
        secondIdentity,
        new ProjectileHandle(new ProjectileSlot(-1), 1)),
      "Negative projectile slots must be rejected.");
    AssertThrows<ArgumentOutOfRangeException>(
      () => index.TryRegister(
        secondIdentity,
        new ProjectileHandle(new ProjectileSlot(1), 0)),
      "Generation zero must be rejected.");

    Assert(
      index.TryUnregister(conflictHandle, out foundIdentity),
      "Current handle should unregister.");
    AssertEqual(secondIdentity, foundIdentity, "Unregister should return the removed identity.");
    AssertEqual(0, index.Count, "Unregister should remove both mapping directions.");
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void AssertEqual<T>(T expected, T actual, string message)
  {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
      throw new InvalidOperationException(
        $"{message} Expected '{expected}', actual '{actual}'.");
    }
  }

  private static void AssertThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
