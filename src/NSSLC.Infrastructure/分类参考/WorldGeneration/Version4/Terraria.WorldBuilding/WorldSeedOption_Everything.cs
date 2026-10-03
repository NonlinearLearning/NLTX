using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.WorldBuilding;

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

	private void UpdateDependentState(AWorldGenerationOption changed){}
	protected override void OnEnabledStateChanged(){}
	public override UIElement ProvideUIElement(){
  return new UIElement();
}
}
