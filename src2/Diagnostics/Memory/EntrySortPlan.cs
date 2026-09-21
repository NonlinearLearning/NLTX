namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class EntrySortPlan<TStep>
{
  private readonly List<TStep> _steps = new();

  public IReadOnlyList<TStep> Steps => _steps;

  public void AddStep(TStep step)
  {
    _steps.Add(step);
  }
}
