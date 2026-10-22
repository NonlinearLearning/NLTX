using System;
using System.Collections.Generic;
using MinimalImplementation.Persistence;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Captures the Host-composed server tick registrations before scheduler construction.
/// </summary>
/// <remarks>
/// This catalog reports phase coverage and registration shape without creating owners. Domain
/// owners remain responsible for their authoritative facts and effects; the scheduler remains
/// the final validator for transaction identity and execution invariants.
/// </remarks>
public sealed class ServerTickPhaseOwnerCatalog
{
  private readonly IReadOnlyList<IServerTickPhaseOwner> _owners;
  private readonly IReadOnlyList<ServerTickPhase> _missingPhases;
  private readonly IReadOnlyList<ServerTickPhase> _duplicatePhases;
  private readonly IReadOnlyList<string> _registrationErrors;

  private ServerTickPhaseOwnerCatalog(
    IReadOnlyList<IServerTickPhaseOwner> owners,
    IReadOnlyList<ServerTickPhase> missingPhases,
    IReadOnlyList<ServerTickPhase> duplicatePhases,
    IReadOnlyList<string> registrationErrors)
  {
    _owners = owners;
    _missingPhases = missingPhases;
    _duplicatePhases = duplicatePhases;
    _registrationErrors = registrationErrors;
  }

  public IReadOnlyList<IServerTickPhaseOwner> Owners => _owners;

  public IReadOnlyList<ServerTickPhase> MissingPhases => _missingPhases;

  public IReadOnlyList<ServerTickPhase> DuplicatePhases => _duplicatePhases;

  public IReadOnlyList<string> RegistrationErrors => _registrationErrors;

  public bool IsComplete => MissingPhases.Count == 0
    && DuplicatePhases.Count == 0
    && RegistrationErrors.Count == 0;

  public static ServerTickPhaseOwnerCatalog Capture(
    IReadOnlyCollection<IServerTickPhaseOwner> owners)
  {
    ArgumentNullException.ThrowIfNull(owners);

    IServerTickPhaseOwner[] ownerSnapshot =
      new List<IServerTickPhaseOwner>(owners).ToArray();
    var coveredPhases = new HashSet<ServerTickPhase>();
    var duplicatePhases = new HashSet<ServerTickPhase>();
    var registrationErrors = new List<string>();

    foreach (IServerTickPhaseOwner? owner in ownerSnapshot)
    {
      if (owner is null)
      {
        registrationErrors.Add("The tick owner collection contains a null owner.");
        continue;
      }

      if (owner is not IServerTickPhaseCompositeOwner composite)
      {
        RegisterPhase(
          owner.Phase,
          owner.GetType().Name,
          coveredPhases,
          duplicatePhases,
          registrationErrors);
        continue;
      }

      IReadOnlyList<IServerTickPhaseParticipant>? participants = composite.Participants;
      if (participants is null || participants.Count == 0)
      {
        registrationErrors.Add(
          $"Composite owner '{owner.GetType().Name}' has no declared phase participants.");
        continue;
      }

      if (participants[0] is null || participants[0].Phase != owner.Phase)
      {
        registrationErrors.Add(
          $"Composite owner '{owner.GetType().Name}' does not anchor its first participant " +
          $"at phase '{owner.Phase}'.");
      }

      foreach (IServerTickPhaseParticipant? participant in participants)
      {
        if (participant is null)
        {
          registrationErrors.Add(
            $"Composite owner '{owner.GetType().Name}' contains a null phase participant.");
          continue;
        }

        RegisterPhase(
          participant.Phase,
          $"{owner.GetType().Name}.{participant.Phase}",
          coveredPhases,
          duplicatePhases,
          registrationErrors);
      }
    }

    var missingPhases = new List<ServerTickPhase>();
    foreach (ServerTickPhase phase in Enum.GetValues<ServerTickPhase>())
    {
      if (!coveredPhases.Contains(phase))
      {
        missingPhases.Add(phase);
      }
    }

    var orderedDuplicatePhases = new List<ServerTickPhase>();
    foreach (ServerTickPhase phase in Enum.GetValues<ServerTickPhase>())
    {
      if (duplicatePhases.Contains(phase))
      {
        orderedDuplicatePhases.Add(phase);
      }
    }

    return new ServerTickPhaseOwnerCatalog(
      Array.AsReadOnly(ownerSnapshot),
      Array.AsReadOnly(missingPhases.ToArray()),
      Array.AsReadOnly(orderedDuplicatePhases.ToArray()),
      Array.AsReadOnly(registrationErrors.ToArray()));
  }

  public string DescribeBlocker()
  {
    if (IsComplete)
    {
      return string.Empty;
    }

    var details = new List<string>();
    if (MissingPhases.Count > 0)
    {
      details.Add($"missing phases: {string.Join(", ", MissingPhases)}");
    }

    if (DuplicatePhases.Count > 0)
    {
      details.Add($"duplicate phases: {string.Join(", ", DuplicatePhases)}");
    }

    if (RegistrationErrors.Count > 0)
    {
      details.Add($"registration errors: {string.Join(" | ", RegistrationErrors)}");
    }

    return $"The supplied tick phase owner catalog is incomplete ({string.Join("; ", details)}).";
  }

  private static void RegisterPhase(
    ServerTickPhase phase,
    string source,
    ISet<ServerTickPhase> coveredPhases,
    ISet<ServerTickPhase> duplicatePhases,
    ICollection<string> registrationErrors)
  {
    if (!Enum.IsDefined(phase))
    {
      registrationErrors.Add($"Owner '{source}' declares undefined phase '{phase}'.");
      return;
    }

    if (!coveredPhases.Add(phase))
    {
      duplicatePhases.Add(phase);
    }
  }
}
