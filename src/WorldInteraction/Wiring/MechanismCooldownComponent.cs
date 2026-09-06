using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.Wiring;

public sealed class MechanismCooldownComponent
{
  private readonly List<MechanismCooldownEntry> _entries = new();
  private readonly ReadOnlyCollection<MechanismCooldownEntry> _entriesView;

  public MechanismCooldownComponent()
  {
    _entriesView = _entries.AsReadOnly();
  }

  public IReadOnlyList<MechanismCooldownEntry> Entries => _entriesView;
  public int CannonCooldownTicks { get; internal set; }
  public int BunnyCannonCooldownTicks { get; internal set; }
  public int SnowballCannonCooldownTicks { get; internal set; }
}
