using System;
using System.Collections.Generic;
using System.Linq;
using NSSLC.WorldGeneration.GameContent.UI.Elements;
using NSSLC.WorldGeneration.UI;

namespace NSSLC.WorldGeneration.WorldBuilding;

public class WorldSeedOption_Everything : AWorldGenerationOption
{
	protected List<AWorldGenerationOption> _dependencies;

	protected override string KeyName => "Seed_Everything";

	public override string ServerConfigName => "zenith";

	public List<AWorldGenerationOption> Dependencies
	{
		get
		{
			if (_dependencies == null)
			{
				_dependencies = new List<AWorldGenerationOption>
				{
					WorldGenerationOptions.Get<WorldSeedOption_Remix>(),
					WorldGenerationOptions.Get<WorldSeedOption_Drunk>(),
					WorldGenerationOptions.Get<WorldSeedOption_NotTheBees>(),
					WorldGenerationOptions.Get<WorldSeedOption_NoTraps>(),
					WorldGenerationOptions.Get<WorldSeedOption_DontStarve>(),
					WorldGenerationOptions.Get<WorldSeedOption_Anniversary>(),
					WorldGenerationOptions.Get<WorldSeedOption_ForTheWorthy>()
				};
			}
			return _dependencies;
		}
	}

	public WorldSeedOption_Everything()
	{
		base.SpecialSeedNames = new string[1] { "getfixedboi" };
		base.SpecialSeedValues = new int[0];
		AWorldGenerationOption.OnOptionStateChanged += UpdateDependentState;
	}

	private void UpdateDependentState(AWorldGenerationOption changed)
{
	
		if (Dependencies.Contains(changed) && changed.Enabled != base.Enabled)
		{
			base.Enabled = Dependencies.All((AWorldGenerationOption d) => d.Enabled);
		}
	
	}
	protected override void OnEnabledStateChanged()
{
	
		if (!base.Enabled && Dependencies.Any((AWorldGenerationOption d) => !d.Enabled))
		{
			return;
		}
		foreach (AWorldGenerationOption dependency in Dependencies)
		{
			dependency.Enabled = base.Enabled;
		}
	
	}
	public override UIElement ProvideUIElement()
{
    throw new NotSupportedException("This world generator has no UI host.");
  }}
