namespace Terraria.SpatialMotionPhysics;

public static class ReferenceProtocolAdapter
{
  private const int EmptyReference = -1;

  public static void Write(
    Stream stream,
    TrackedProjectileReferenceValue reference)
  {
    ArgumentNullException.ThrowIfNull(stream);
    using BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
    writer.Write(reference.ProjectileLocalIndex);
    writer.Write(reference.ProjectileOwnerIndex);
    writer.Write(reference.ProjectileIdentity);
    writer.Write(reference.ProjectileType);
  }

  public static bool TryRead(
    Stream stream,
    out TrackedProjectileReferenceValue reference)
  {
    ArgumentNullException.ThrowIfNull(stream);
    try
    {
      using BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
      reference = new TrackedProjectileReferenceValue(
        reader.ReadInt32(),
        reader.ReadInt32(),
        reader.ReadInt32(),
        reader.ReadInt32());
      if (!TrackedProjectileReferenceQuery.IsTracking(reference))
      {
        reference = Empty();
        return false;
      }

      return true;
    }
    catch (EndOfStreamException)
    {
      reference = Empty();
      return false;
    }
    catch (IOException)
    {
      reference = Empty();
      return false;
    }
  }

  private static TrackedProjectileReferenceValue Empty()
  {
    return new TrackedProjectileReferenceValue(
      EmptyReference,
      EmptyReference,
      EmptyReference,
      EmptyReference);
  }
}
