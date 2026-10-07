using System;
using System.Collections.Generic;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

internal static class WorldLoadApiCatalogValidation
{
  internal static bool TryValidate(
    IReadOnlyList<WorldLoadApiDescriptor> descriptors,
    int formatVersion,
    IReadOnlyCollection<string> availableSectionIds,
    out WorldLoadApiFailure failure,
    out string? failureApiId,
    out string? failureOwnerId)
  {
    ArgumentNullException.ThrowIfNull(descriptors);
    ArgumentNullException.ThrowIfNull(availableSectionIds);
    if (formatVersion < 0)
    {
      failureApiId = null;
      failureOwnerId = null;
      failure = WorldLoadApiFailure.Create(
        "InvalidFormatVersion",
        "The world persistence document has a negative format version.");
      return false;
    }

    failureApiId = null;
    failureOwnerId = null;
    var apiIds = new HashSet<string>(StringComparer.Ordinal);
    var consumedSectionIds = new HashSet<string>(StringComparer.Ordinal);
    var availableSections = new HashSet<string>(availableSectionIds, StringComparer.Ordinal);
    var byApiId = new Dictionary<string, WorldLoadApiDescriptor>(StringComparer.Ordinal);

    foreach (WorldLoadApiDescriptor? descriptor in descriptors)
    {
      if (descriptor is null)
      {
        failure = WorldLoadApiFailure.Create(
          "InvalidApiCatalog",
          "The world load API catalog contained a null descriptor.");
        return false;
      }

      if (string.IsNullOrWhiteSpace(descriptor.ApiId) ||
          string.IsNullOrWhiteSpace(descriptor.OwnerId) ||
          string.IsNullOrWhiteSpace(descriptor.SectionId) ||
          descriptor.CommitAfter is null ||
          descriptor.MinimumFormatVersion < 0 ||
          descriptor.MaximumFormatVersion < descriptor.MinimumFormatVersion ||
          descriptor.Requirement is not WorldLoadSectionRequirement.Required and
            not WorldLoadSectionRequirement.Optional)
      {
        failureApiId = descriptor.ApiId;
        failureOwnerId = descriptor.OwnerId;
        failure = WorldLoadApiFailure.Create(
          "InvalidApiCatalog",
          "The world load API catalog contained an invalid descriptor.");
        return false;
      }

      var dependencyIds = new HashSet<string>(StringComparer.Ordinal);
      foreach (string? dependency in descriptor.CommitAfter)
      {
        if (string.IsNullOrWhiteSpace(dependency) ||
            string.Equals(dependency, descriptor.ApiId, StringComparison.Ordinal) ||
            !dependencyIds.Add(dependency))
        {
          failureApiId = descriptor.ApiId;
          failureOwnerId = descriptor.OwnerId;
          failure = WorldLoadApiFailure.Create(
            "InvalidApiCatalog",
            $"API '{descriptor.ApiId}' contains an invalid CommitAfter declaration.");
          return false;
        }
      }

      if (!apiIds.Add(descriptor.ApiId))
      {
        failureApiId = descriptor.ApiId;
        failureOwnerId = descriptor.OwnerId;
        failure = WorldLoadApiFailure.Create(
          "DuplicateApiId",
          $"The world load API catalog contains duplicate ApiId '{descriptor.ApiId}'.");
        return false;
      }

      if (!consumedSectionIds.Add(descriptor.SectionId))
      {
        failureApiId = descriptor.ApiId;
        failureOwnerId = descriptor.OwnerId;
        failure = WorldLoadApiFailure.Create(
          "DuplicateSectionId",
          $"The world load API catalog contains duplicate SectionId '{descriptor.SectionId}'.");
        return false;
      }

      if (formatVersion < descriptor.MinimumFormatVersion ||
          formatVersion > descriptor.MaximumFormatVersion)
      {
        failureApiId = descriptor.ApiId;
        failureOwnerId = descriptor.OwnerId;
        failure = WorldLoadApiFailure.Create(
          "UnsupportedFormatVersion",
          $"API '{descriptor.ApiId}' does not support format version {formatVersion}.");
        return false;
      }

      if (descriptor.Requirement == WorldLoadSectionRequirement.Required &&
          !availableSections.Contains(descriptor.SectionId))
      {
        failureApiId = descriptor.ApiId;
        failureOwnerId = descriptor.OwnerId;
        failure = WorldLoadApiFailure.Create(
          "MissingRequiredSection",
          $"Required world section '{descriptor.SectionId}' is absent.");
        return false;
      }

      byApiId.Add(descriptor.ApiId, descriptor);
    }

    foreach (string sectionId in new SortedSet<string>(availableSections, StringComparer.Ordinal))
    {
      if (consumedSectionIds.Contains(sectionId))
      {
        continue;
      }

      failure = WorldLoadApiFailure.Create(
        "UnconsumedSection",
        $"World section '{sectionId}' has no registered load API.");
      return false;
    }

    var indegree = new Dictionary<string, int>(StringComparer.Ordinal);
    var dependents = new Dictionary<string, List<string>>(StringComparer.Ordinal);
    foreach (string apiId in apiIds)
    {
      indegree.Add(apiId, 0);
      dependents.Add(apiId, new List<string>());
    }

    foreach (WorldLoadApiDescriptor descriptor in descriptors)
    {
      foreach (string dependency in descriptor.CommitAfter)
      {
        if (!apiIds.Contains(dependency))
        {
          failureApiId = descriptor.ApiId;
          failureOwnerId = descriptor.OwnerId;
          failure = WorldLoadApiFailure.Create(
            "MissingDependency",
            $"API '{descriptor.ApiId}' depends on unknown ApiId '{dependency}'.");
          return false;
        }

        indegree[descriptor.ApiId]++;
        dependents[dependency].Add(descriptor.ApiId);
      }
    }

    var ready = new SortedSet<string>(
      StringComparer.Ordinal);
    foreach (KeyValuePair<string, int> entry in indegree)
    {
      if (entry.Value == 0)
      {
        ready.Add(entry.Key);
      }
    }

    int processed = 0;
    var processedApiIds = new HashSet<string>(StringComparer.Ordinal);
    while (ready.Count > 0)
    {
      string apiId = ready.Min!;
      ready.Remove(apiId);
      processed++;
      processedApiIds.Add(apiId);
      foreach (string dependent in dependents[apiId])
      {
        indegree[dependent]--;
        if (indegree[dependent] == 0)
        {
          ready.Add(dependent);
        }
      }
    }

    if (processed != byApiId.Count)
    {
      WorldLoadApiDescriptor? cycleDescriptor = null;
      foreach (WorldLoadApiDescriptor descriptor in descriptors)
      {
        if (processedApiIds.Contains(descriptor.ApiId) ||
            !IsInDependencyCycle(descriptor.ApiId, byApiId))
        {
          continue;
        }

        if (cycleDescriptor is null ||
            string.CompareOrdinal(descriptor.ApiId, cycleDescriptor.ApiId) < 0)
        {
          cycleDescriptor = descriptor;
        }
      }

      if (cycleDescriptor is not null)
      {
        failureApiId = cycleDescriptor.ApiId;
        failureOwnerId = cycleDescriptor.OwnerId;
      }

      failure = WorldLoadApiFailure.Create(
        "DependencyCycle",
        "The world load API catalog contains a CommitAfter dependency cycle.");
      return false;
    }

    failure = default;
    return true;
  }

  private static bool IsInDependencyCycle(
    string apiId,
    IReadOnlyDictionary<string, WorldLoadApiDescriptor> descriptorsById)
  {
    var visited = new HashSet<string>(StringComparer.Ordinal);
    var pending = new Stack<string>(descriptorsById[apiId].CommitAfter);
    while (pending.Count > 0)
    {
      string currentApiId = pending.Pop();
      if (string.Equals(currentApiId, apiId, StringComparison.Ordinal))
      {
        return true;
      }

      if (!visited.Add(currentApiId))
      {
        continue;
      }

      foreach (string dependency in descriptorsById[currentApiId].CommitAfter)
      {
        pending.Push(dependency);
      }
    }

    return false;
  }
}
