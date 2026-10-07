using System;
using Terraria.Relationships;

namespace EntityEcs;

public readonly struct EntityComponentSnapshot<TProjection>
  where TProjection : struct
{
  internal EntityComponentSnapshot(
    RuntimeEntityHandle handle,
    Type componentType,
    long attachmentRevision,
    long dataRevision,
    TProjection value)
  {
    Handle = handle;
    ComponentType = componentType;
    AttachmentRevision = attachmentRevision;
    DataRevision = dataRevision;
    Value = value;
  }

  public TProjection Value { get; }

  public long AttachmentRevision { get; }

  public long DataRevision { get; }

  internal RuntimeEntityHandle Handle { get; }

  internal Type ComponentType { get; }
}

public delegate void EntityComponentEditor<TComponent>(ref TComponent component);

/// <summary>
/// Synchronously inspects a component without committing a data revision.
/// Implementations must not retain references to mutable component storage.
/// </summary>
public delegate void EntityComponentInspector<TComponent>(in TComponent component);

public delegate void EntityComponentPairEditor<TFirstComponent, TSecondComponent>(
  ref TFirstComponent firstComponent,
  ref TSecondComponent secondComponent);

public delegate void EntityComponentTripleEditor<
  TFirstComponent,
  TSecondComponent,
  TThirdComponent>(
  ref TFirstComponent firstComponent,
  ref TSecondComponent secondComponent,
  ref TThirdComponent thirdComponent);

public delegate void EntityComponentFiveEditor<
  TFirstComponent,
  TSecondComponent,
  TThirdComponent,
  TFourthComponent,
  TFifthComponent>(
  ref TFirstComponent firstComponent,
  ref TSecondComponent secondComponent,
  ref TThirdComponent thirdComponent,
  ref TFourthComponent fourthComponent,
  ref TFifthComponent fifthComponent);

public delegate void EntityComponentSixEditor<
  TFirstComponent,
  TSecondComponent,
  TThirdComponent,
  TFourthComponent,
  TFifthComponent,
  TSixthComponent>(
  ref TFirstComponent firstComponent,
  ref TSecondComponent secondComponent,
  ref TThirdComponent thirdComponent,
  ref TFourthComponent fourthComponent,
  ref TFifthComponent fifthComponent,
  ref TSixthComponent sixthComponent);
