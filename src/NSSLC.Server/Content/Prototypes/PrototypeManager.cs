using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NSSLC.Server.Content.Prototypes;

/// <summary>
/// Loads external content prototypes before the ECS server starts.
/// </summary>
/// <remarks>
/// This is intentionally a small content boundary for item-use data. It does not discover or
/// construct AI behaviors. Parsed prototypes are returned to the caller, which then projects them
/// into an immutable domain catalog.
/// </remarks>
internal sealed class PrototypeManager
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
  };

  public IReadOnlyList<TPrototype> LoadDirectory<TPrototype>(string directory)
      where TPrototype : class, IPrototype
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(directory);
    if (!Directory.Exists(directory))
    {
      throw new DirectoryNotFoundException(
          $"Prototype directory '{directory}' does not exist.");
    }

    PrototypeAttribute prototypeAttribute = typeof(TPrototype)
        .GetCustomAttributes(typeof(PrototypeAttribute), inherit: false)
        .OfType<PrototypeAttribute>()
        .SingleOrDefault()
        ?? throw new InvalidOperationException(
            $"Prototype type '{typeof(TPrototype).FullName}' is missing [Prototype].");

    List<TPrototype> prototypes = [];
    HashSet<string> ids = new(StringComparer.Ordinal);
    string[] files = Directory
        .EnumerateFiles(directory, "*.json", SearchOption.AllDirectories)
        .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    if (files.Length == 0)
    {
      throw new InvalidDataException(
          $"Prototype directory '{directory}' does not contain any JSON files.");
    }

    foreach (string file in files)
    {
      string json = File.ReadAllText(file);
      List<TPrototype>? filePrototypes;
      try
      {
        filePrototypes = JsonSerializer.Deserialize<List<TPrototype>>(json, JsonOptions);
      }
      catch (JsonException exception)
      {
        throw new InvalidDataException(
            $"Could not parse prototype file '{file}': {exception.Message}", exception);
      }

      if (filePrototypes is null)
      {
        throw new InvalidDataException($"Prototype file '{file}' contains no prototype list.");
      }

      for (int index = 0; index < filePrototypes.Count; index++)
      {
        TPrototype prototype = filePrototypes[index]
            ?? throw new InvalidDataException(
                $"Prototype file '{file}' contains a null entry at index {index}.");
        ValidatePrototype(prototype, prototypeAttribute.Type, file, index);
        if (!ids.Add(prototype.Id))
        {
          throw new InvalidDataException(
              $"Prototype '{prototype.Id}' is declared more than once; " +
              $"the duplicate is in '{file}' at index {index}.");
        }

        prototypes.Add(prototype);
      }
    }

    return prototypes.AsReadOnly();
  }

  private static void ValidatePrototype<TPrototype>(
      TPrototype prototype,
      string expectedType,
      string file,
      int index)
      where TPrototype : class, IPrototype
  {
    if (string.IsNullOrWhiteSpace(prototype.Id))
    {
      throw new InvalidDataException(
          $"Prototype file '{file}' entry {index} has an empty id.");
    }

    if (prototype is ItemUseDefinitionPrototype itemUse &&
        !string.Equals(itemUse.Type, expectedType, StringComparison.Ordinal))
    {
      throw new InvalidDataException(
          $"Prototype file '{file}' entry {index} has type '{itemUse.Type}', " +
          $"expected '{expectedType}'.");
    }
  }
}

