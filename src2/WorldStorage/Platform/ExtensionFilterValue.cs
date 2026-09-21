using System.Collections.ObjectModel;

namespace Terraria.NonAuthoritative.Platform;

public sealed class ExtensionFilterValue
{
  private readonly ReadOnlyCollection<string> _extensions;

  public ExtensionFilterValue(string name, IEnumerable<string> extensions)
  {
    Name = string.IsNullOrWhiteSpace(name)
      ? throw new ArgumentException("A filter name is required.", nameof(name))
      : name;
    ArgumentNullException.ThrowIfNull(extensions);

    string[] extensionCopy = extensions.ToArray();
    if (extensionCopy.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException("Filter extensions cannot be empty.", nameof(extensions));
    }

    _extensions = Array.AsReadOnly(extensionCopy);
  }

  public string Name { get; }

  public IReadOnlyList<string> Extensions => _extensions;
}
