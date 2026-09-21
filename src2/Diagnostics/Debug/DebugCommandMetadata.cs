namespace Terraria.NonAuthoritative.Diagnostics;

public sealed record DebugCommandMetadata
{
  public DebugCommandMetadata(
    string name,
    string description,
    string helpText,
    CommandRequirement requirements)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("A command name is required.", nameof(name));
    }

    Name = name;
    Description = description ?? string.Empty;
    HelpText = helpText ?? string.Empty;
    Requirements = requirements;
  }

  public string Name { get; }

  public string Description { get; }

  public string HelpText { get; }

  public CommandRequirement Requirements { get; }
}
