using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

const int ExpectedTotalWeight = 100;
string manifestPath = Path.Combine(
  Directory.GetCurrentDirectory(),
  "docs",
  "server-completion",
  "completion-manifest.json");
CompletionManifest manifest = CompletionManifest.Load(manifestPath);

if (manifest.SchemaVersion != 1)
{
  throw new InvalidOperationException("Completion manifest schema version is not supported.");
}

int totalWeight = 0;
int evidencedScore = 0;
HashSet<string> familyIds = new(StringComparer.Ordinal);
foreach (CompletionFamily family in manifest.Families)
{
  if (!familyIds.Add(family.Id))
  {
    throw new InvalidOperationException($"Completion manifest contains duplicate family '{family.Id}'.");
  }

  if (family.Weight <= 0)
  {
    throw new InvalidOperationException($"Completion family '{family.Id}' has an invalid weight.");
  }

  totalWeight += family.Weight;
  if (family.Status == CompletionStatus.Evidenced)
  {
    family.ValidateEvidence();
    evidencedScore += family.Weight;
  }
  else if (family.Status != CompletionStatus.Missing && family.Status != CompletionStatus.Partial)
  {
    throw new InvalidOperationException($"Completion family '{family.Id}' has an invalid status.");
  }
}

if (totalWeight != ExpectedTotalWeight)
{
  throw new InvalidOperationException(
    $"Completion manifest weights total {totalWeight}, not {ExpectedTotalWeight}.");
}

VerifyUnsupportedClaimIsRejected();
Console.WriteLine($"PASS: completion manifest totals {totalWeight} points");
Console.WriteLine($"PASS: evidenced core-server score is {evidencedScore} points");

static void VerifyUnsupportedClaimIsRejected()
{
  CompletionFamily unsupportedClaim = new(
    "X",
    "Unsupported claim",
    1,
    CompletionStatus.Evidenced,
    null,
    null,
    null,
    null);
  try
  {
    unsupportedClaim.ValidateEvidence();
  }
  catch (InvalidOperationException)
  {
    return;
  }

  throw new InvalidOperationException(
    "Completion verifier accepted an evidenced family without all required proof fields.");
}

internal sealed class CompletionManifest
{
  public int SchemaVersion { get; init; }
  public List<CompletionFamily> Families { get; init; } = new();

  public static CompletionManifest Load(string manifestPath)
  {
    if (!File.Exists(manifestPath))
    {
      throw new FileNotFoundException("Completion manifest was not found.", manifestPath);
    }

    string content = File.ReadAllText(manifestPath);
    CompletionManifest? manifest = JsonSerializer.Deserialize<CompletionManifest>(
      content,
      new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
      });
    return manifest ?? throw new InvalidOperationException("Completion manifest is empty.");
  }
}

internal sealed class CompletionFamily
{
  public CompletionFamily(
    string id,
    string name,
    int weight,
    CompletionStatus status,
    string? reference = null,
    string? authority = null,
    string? projection = null,
    string? verifierCommand = null)
  {
    Id = id;
    Name = name;
    Weight = weight;
    Status = status;
    Reference = reference;
    Authority = authority;
    Projection = projection;
    VerifierCommand = verifierCommand;
  }

  public string Id { get; init; }
  public string Name { get; init; }
  public int Weight { get; init; }
  public CompletionStatus Status { get; init; }
  public string? Reference { get; init; }
  public string? Authority { get; init; }
  public string? Projection { get; init; }
  public string? VerifierCommand { get; init; }

  public void ValidateEvidence()
  {
    if (string.IsNullOrWhiteSpace(Reference) ||
        string.IsNullOrWhiteSpace(Authority) ||
        string.IsNullOrWhiteSpace(Projection) ||
        string.IsNullOrWhiteSpace(VerifierCommand))
    {
      throw new InvalidOperationException(
        $"Evidenced completion family '{Id}' is missing required proof fields.");
    }
  }
}

internal enum CompletionStatus
{
  Missing,
  Partial,
  Evidenced
}
