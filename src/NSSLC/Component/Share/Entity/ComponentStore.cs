using System;
using System.Reflection;
using Terraria.Relationships;

namespace EntityEcs;

internal interface IComponentStore
{
  bool Remove(RuntimeEntityHandle handle);
}

internal sealed class ComponentStore<TComponent> : IComponentStore
  where TComponent : notnull
{
  private ComponentCell<TComponent>?[] _cells = Array.Empty<ComponentCell<TComponent>>();
  private long _lastAttachmentRevision;

  public bool Contains(RuntimeEntityHandle handle) => TryGetCell(handle, out _);

  public bool TryAttach(RuntimeEntityHandle handle, TComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    EnsureCapacity(handle.LocalIndex);
    if (_cells[handle.LocalIndex] is not null || IsAttachedElsewhere(handle, component))
    {
      return false;
    }

    long attachmentRevision = NextAttachmentRevision();
    _cells[handle.LocalIndex] = new ComponentCell<TComponent>(
        handle.LocalIndex,
        handle.Generation,
        component,
        attachmentRevision);
    _lastAttachmentRevision = attachmentRevision;
    return true;
  }

  public bool TryReplace(RuntimeEntityHandle handle, TComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell))
    {
      return false;
    }

    if (IsAttachedElsewhere(handle, component))
    {
      return false;
    }

    long nextAttachmentRevision = NextAttachmentRevision();
    long nextDataRevision = checked(cell.DataRevision + 1);
    cell.Value = component;
    cell.AttachmentRevision = nextAttachmentRevision;
    cell.DataRevision = nextDataRevision;
    _lastAttachmentRevision = nextAttachmentRevision;
    return true;
  }

  public bool TryReplace<TProjection>(
    RuntimeEntityHandle handle,
    EntityComponentSnapshot<TProjection> expectedSnapshot,
    TComponent component)
    where TProjection : struct
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell) ||
        expectedSnapshot.Handle != handle ||
        expectedSnapshot.ComponentType != typeof(TComponent) ||
        expectedSnapshot.AttachmentRevision != cell.AttachmentRevision ||
        expectedSnapshot.DataRevision != cell.DataRevision ||
        IsAttachedElsewhere(handle, component))
    {
      return false;
    }

    long nextAttachmentRevision = NextAttachmentRevision();
    long nextDataRevision = checked(cell.DataRevision + 1);
    cell.Value = component;
    cell.AttachmentRevision = nextAttachmentRevision;
    cell.DataRevision = nextDataRevision;
    _lastAttachmentRevision = nextAttachmentRevision;
    return true;
  }

  public bool TryEdit(RuntimeEntityHandle handle, EntityComponentEditor<TComponent> editor)
  {
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell))
    {
      return false;
    }

    TComponent original = cell.Value;
    try
    {
      editor(ref cell.Value);
    }
    finally
    {
      cell.DataRevision = checked(cell.DataRevision + 1);
    }

    if (!typeof(TComponent).IsValueType && !ReferenceEquals(original, cell.Value))
    {
      cell.Value = original;
      return false;
    }

    return true;
  }

  public bool TryInspect(
    RuntimeEntityHandle handle,
    EntityComponentInspector<TComponent> inspector)
  {
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell))
    {
      return false;
    }

    inspector(in cell.Value);
    return true;
  }

  public bool TryCapture<TProjection>(
    RuntimeEntityHandle handle,
    Func<TComponent, TProjection> capture,
    out TProjection projection)
    where TProjection : struct
  {
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell))
    {
      projection = default;
      return false;
    }

    projection = capture(cell.Value);
    return true;
  }

  public bool TryCaptureVersioned<TProjection>(
    RuntimeEntityHandle handle,
    Func<TComponent, TProjection> capture,
    out TProjection projection,
    out long attachmentRevision,
    out long dataRevision)
    where TProjection : struct
  {
    if (!TryGetCell(handle, out ComponentCell<TComponent> cell))
    {
      projection = default;
      attachmentRevision = default;
      dataRevision = default;
      return false;
    }

    projection = capture(cell.Value);
    attachmentRevision = cell.AttachmentRevision;
    dataRevision = cell.DataRevision;
    return true;
  }

  public bool Remove(RuntimeEntityHandle handle)
  {
    if (!TryGetCell(handle, out _))
    {
      return false;
    }

    _cells[handle.LocalIndex] = null;
    return true;
  }

  private bool IsAttachedElsewhere(RuntimeEntityHandle handle, TComponent component)
  {
    if (typeof(TComponent).IsValueType)
    {
      return false;
    }

    foreach (ComponentCell<TComponent>? cell in _cells)
    {
      if (cell is not null &&
          (cell.LocalIndex != handle.LocalIndex || cell.Generation != handle.Generation) &&
          ReferenceEquals(cell.Value, component))
      {
        return true;
      }
    }

    return false;
  }

  private long NextAttachmentRevision() => checked(_lastAttachmentRevision + 1);

  private void EnsureCapacity(int index)
  {
    if (index < _cells.Length)
    {
      return;
    }

    int capacity = Math.Max(index + 1, Math.Max(4, _cells.Length * 2));
    Array.Resize(ref _cells, capacity);
  }

  private bool TryGetCell(RuntimeEntityHandle handle, out ComponentCell<TComponent> cell)
  {
    if ((uint)handle.LocalIndex < (uint)_cells.Length &&
        _cells[handle.LocalIndex] is ComponentCell<TComponent> candidate &&
        candidate.Generation == handle.Generation)
    {
      cell = candidate;
      return true;
    }

    cell = null!;
    return false;
  }

  private sealed class ComponentCell<TValue>
  {
    public ComponentCell(
      int localIndex,
      uint generation,
      TValue value,
      long attachmentRevision)
    {
      LocalIndex = localIndex;
      Generation = generation;
      Value = value;
      AttachmentRevision = attachmentRevision;
    }

    public int LocalIndex { get; }

    public uint Generation { get; }

    public TValue Value;

    public long AttachmentRevision { get; set; }

    public long DataRevision { get; set; } = 1;
  }
}

internal static class EntitySnapshotProjection<TProjection>
  where TProjection : struct
{
  private static readonly bool IsSafe = IsSafeValueType(typeof(TProjection));

  public static void EnsureSafe()
  {
    if (!IsSafe)
    {
      throw new InvalidOperationException(
        $"Snapshot projection {typeof(TProjection)} contains a mutable reference. " +
        "Capture value fields or immutable strings instead.");
    }
  }

  private static bool IsSafeValueType(Type type)
  {
    if (type == typeof(string) || type.IsPrimitive || type.IsEnum)
    {
      return true;
    }

    if (!type.IsValueType || type.IsByRefLike || type.IsPointer || type.IsByRef)
    {
      return false;
    }

    foreach (FieldInfo field in type.GetFields(
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
      if (!IsSafeValueType(field.FieldType))
      {
        return false;
      }
    }

    return true;
  }
}
