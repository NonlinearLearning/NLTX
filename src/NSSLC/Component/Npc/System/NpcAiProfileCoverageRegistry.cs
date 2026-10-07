namespace Terraria.Npc;

public sealed class NpcAiProfileCoverageRegistry
{
  public const int MinimumIndexedStyle = 0;
  public const int MaximumIndexedStyle = 127;

  private readonly HashSet<int> _indexedStyles;
  private readonly Dictionary<NpcAiProfileIdentity, NpcAiProfileCoverageRegistration>
    _registrations = [];

  public NpcAiProfileCoverageRegistry(IEnumerable<int> indexedStyles)
  {
    ArgumentNullException.ThrowIfNull(indexedStyles);
    _indexedStyles = [];
    foreach (int aiStyle in indexedStyles)
    {
      if (aiStyle < MinimumIndexedStyle || aiStyle > MaximumIndexedStyle)
      {
        throw new ArgumentOutOfRangeException(
          nameof(indexedStyles),
          aiStyle,
          $"NPC AI style must be between {MinimumIndexedStyle} and {MaximumIndexedStyle}.");
      }

      if (!_indexedStyles.Add(aiStyle))
      {
        throw new ArgumentException(
          $"NPC AI style {aiStyle} is indexed more than once.",
          nameof(indexedStyles));
      }
    }
  }

  public IReadOnlyList<int> IndexedStyles => _indexedStyles.Order().ToArray();

  public IReadOnlyList<NpcAiProfileCoverageRegistration> Registrations =>
    _registrations.Values
      .OrderBy(static registration => registration.Identity.AiStyle)
      .ThenBy(static registration => registration.Identity.TypeId)
      .ThenBy(static registration => registration.Identity.NetId)
      .ToArray();

  public void Register(NpcAiProfileCoverageRegistration registration)
  {
    ArgumentNullException.ThrowIfNull(registration);
    ValidateRegistration(registration);

    if (_registrations.ContainsKey(registration.Identity))
    {
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.DuplicateRegistration,
        registration.Identity,
        $"NPC AI profile {registration.Identity} is registered more than once.");
    }

    NpcAiProfileCoverageRegistration[] conflicts = FindIdentityConflicts(registration.Identity);
    if (conflicts.Length > 0)
    {
      string expected = string.Join(
        "; ",
        conflicts.Select(static conflict => $"{conflict.Name} ({conflict.Identity})"));
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.IdentityConflict,
        registration.Identity,
        $"NPC AI profile registration {registration.Identity} conflicts with " +
        $"registered profile(s): {expected}.");
    }

    _registrations.Add(registration.Identity, registration);
  }

  public NpcAiProfileCoverageRegistration RequireRegistered(NpcAiProfileIdentity identity)
  {
    EnsureStyleIndexed(identity);
    if (_registrations.TryGetValue(identity, out NpcAiProfileCoverageRegistration? registration))
    {
      return registration;
    }

    NpcAiProfileCoverageRegistration[] conflicts = FindIdentityConflicts(identity);
    if (conflicts.Length > 0)
    {
      string expected = string.Join(
        "; ",
        conflicts.Select(static conflict => $"{conflict.Name} ({conflict.Identity})"));
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.IdentityConflict,
        identity,
        $"NPC AI profile identity {identity} conflicts with registered profile(s): {expected}.");
    }

    throw CreateFailure(
      NpcAiProfileRegistrationFailure.UnregisteredProfile,
      identity,
      $"NPC AI profile {identity} is not registered. Shared aiStyle does not register a profile.");
  }

  public NpcAiProfileCoverageRegistration RequireFiniteHandler(NpcAiProfileIdentity identity)
  {
    NpcAiProfileCoverageRegistration registration = RequireRegistered(identity);
    if (!registration.HasFiniteHandler)
    {
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.FiniteHandlerMissing,
        identity,
        $"NPC AI profile {identity} is mapped but has no finite runtime handler registration.");
    }

    return registration;
  }

  public NpcAiProfileCoverageRegistration RequireVerified(NpcAiProfileIdentity identity)
  {
    NpcAiProfileCoverageRegistration registration = RequireRegistered(identity);
    if (registration.Stage != NpcAiCoverageStage.Verified || registration.HasOpenWork)
    {
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.CoverageOpen,
        identity,
        $"NPC AI profile {identity} is {registration.Stage} and " +
        $"{(registration.HasOpenWork ? "has open work" : "has not reached verified")}; " +
        "fallback or finite behavior does not close source coverage.");
    }

    return registration;
  }

  private void ValidateRegistration(NpcAiProfileCoverageRegistration registration)
  {
    if (string.IsNullOrWhiteSpace(registration.Name))
    {
      throw new ArgumentException("NPC AI profile registration requires a name.", nameof(registration));
    }

    if (registration.Identity.TypeId <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(registration),
        registration.Identity.TypeId,
        "NPC AI profile type id must be positive.");
    }

    EnsureStyleIndexed(registration.Identity);
    if (registration.Stage == NpcAiCoverageStage.Indexed)
    {
      throw new ArgumentException(
        "A concrete profile registration must be mapped before it can be registered.",
        nameof(registration));
    }

    if (registration.Stage == NpcAiCoverageStage.Implemented && !registration.HasFiniteHandler)
    {
      throw new ArgumentException(
        "An implemented profile must have a registered runtime handler.",
        nameof(registration));
    }

    if (registration.Stage == NpcAiCoverageStage.Verified &&
        (!registration.HasFiniteHandler || registration.HasOpenWork))
    {
      throw new ArgumentException(
        "A verified profile must have a runtime handler and no open work.",
        nameof(registration));
    }
  }

  private void EnsureStyleIndexed(NpcAiProfileIdentity identity)
  {
    if (identity.AiStyle < MinimumIndexedStyle ||
        identity.AiStyle > MaximumIndexedStyle ||
        !_indexedStyles.Contains(identity.AiStyle))
    {
      throw CreateFailure(
        NpcAiProfileRegistrationFailure.UnsupportedStyle,
        identity,
        $"NPC AI style {identity.AiStyle} is not present in the indexed 0–127 source inventory.");
    }
  }

  private NpcAiProfileCoverageRegistration[] FindIdentityConflicts(
    NpcAiProfileIdentity identity)
  {
    NpcAiProfileCoverageRegistration[] sameTypeAndStyle = _registrations.Values
      .Where(registration => registration.Identity.TypeId == identity.TypeId &&
        registration.Identity.AiStyle == identity.AiStyle)
      .ToArray();
    bool positiveNetIdHasSingleMappedIdentity = identity.NetId >= 0 &&
      sameTypeAndStyle.Length == 1 &&
      sameTypeAndStyle[0].Identity.NetId >= 0;

    return _registrations.Values.Where(registration =>
      {
        bool sameTypeAndNetId = registration.Identity.TypeId == identity.TypeId &&
          registration.Identity.NetId == identity.NetId;
        bool sameNetIdAndStyle = registration.Identity.NetId == identity.NetId &&
          registration.Identity.AiStyle == identity.AiStyle;
        bool sameUniquePositiveTypeAndStyle = positiveNetIdHasSingleMappedIdentity &&
          registration.Identity.TypeId == identity.TypeId &&
          registration.Identity.AiStyle == identity.AiStyle;
        return sameTypeAndNetId || sameNetIdAndStyle || sameUniquePositiveTypeAndStyle;
      })
      .ToArray();
  }

  private static NpcAiProfileRegistrationException CreateFailure(
    NpcAiProfileRegistrationFailure failure,
    NpcAiProfileIdentity identity,
    string message)
  {
    return new NpcAiProfileRegistrationException(failure, identity, message);
  }
}
