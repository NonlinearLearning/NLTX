using System;
using System.Collections.Generic;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public static class ProtocolFactsBootstrap
{
  public static ProtocolInputs Initialize(Func<ProtocolInputs> createCompleteFacts)
  {
    ArgumentNullException.ThrowIfNull(createCompleteFacts);
    if (ProtocolInputs.IsInitialized)
    {
      throw new InvalidOperationException(
        "Protocol facts were already initialized for this process.");
    }

    ProtocolInputs facts = createCompleteFacts();
    if (!ProtocolInputs.IsInitialized || !ReferenceEquals(facts, ProtocolInputs.Instance))
    {
      throw new InvalidOperationException(
        "The facts factory must publish exactly one ProtocolInputs instance.");
    }

    Validate(facts);
    return facts;
  }

  public static void Validate(ProtocolInputs facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    if (!facts.IsServer)
    {
      throw new InvalidOperationException("Server composition requires IsServer=true.");
    }

    if (facts.FrameImportant.Length == 0 ||
        facts.AllowsSaveCompressionBatching.Length != facts.FrameImportant.Length ||
        facts.CatchableTypes.Length == 0)
    {
      throw new InvalidOperationException(
        "Tile, compression, and catchable content facts must be complete and size-compatible.");
    }

    if (facts.SlotCount != 40)
    {
      throw new InvalidOperationException(
        "SlotCount must describe the Version4 travel merchant table (40), not player capacity.");
    }

    if (facts.ModuleCodecs.Count != 15)
    {
      throw new InvalidOperationException("The complete 15-entry module codec map is required.");
    }

    for (ushort moduleId = 0; moduleId < 15; moduleId++)
    {
      if (!facts.ModuleCodecs.ContainsKey(moduleId))
      {
        throw new InvalidOperationException($"Module codec {moduleId} is missing.");
      }
    }

    if (facts.TagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new InvalidOperationException("Tag-effect NPC slot facts exceed the wire range.");
    }
  }
}
