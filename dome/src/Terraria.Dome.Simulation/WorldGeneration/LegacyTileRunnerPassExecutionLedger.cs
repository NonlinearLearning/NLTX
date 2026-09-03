using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyTileRunnerPassExecutionLedger
{
  private readonly List<LegacyTileRunnerInvocationProvenance> _invocations = new();

  public LegacyTileRunnerPassExecutionLedger(
    string passName,
    string recipeName,
    int expectedInvocationCount)
  {
    ArgumentException.ThrowIfNullOrEmpty(passName);
    ArgumentException.ThrowIfNullOrEmpty(recipeName);
    if (expectedInvocationCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expectedInvocationCount));
    }

    PassName = passName;
    RecipeName = recipeName;
    ExpectedInvocationCount = expectedInvocationCount;
  }

  public int ExpectedInvocationCount { get; }
  public string PassName { get; }
  public string RecipeName { get; }
  public int RecordedInvocationCount => _invocations.Count;

  public void Append(LegacyTileRunnerInvocationProvenance provenance)
  {
    ArgumentNullException.ThrowIfNull(provenance);
    if (provenance.PassName != PassName || provenance.RecipeName != RecipeName ||
        provenance.InvocationIndex != _invocations.Count)
    {
      throw new InvalidOperationException(
        "TileRunner execution provenance did not match the ledger sequence.");
    }

    if (_invocations.Count >= ExpectedInvocationCount)
    {
      throw new InvalidOperationException(
        "TileRunner execution ledger exceeded its bounded invocation count.");
    }

    _invocations.Add(provenance);
  }

  public IReadOnlyList<LegacyTileRunnerInvocationProvenance> Complete()
  {
    if (_invocations.Count != ExpectedInvocationCount)
    {
      throw new InvalidOperationException(
        "TileRunner execution ledger did not reach its expected invocation count.");
    }

    return _invocations.AsReadOnly();
  }
}
