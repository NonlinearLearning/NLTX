using System.Linq;

namespace Terraria.WorldBuilding;

public class WorldSeedOption_Normal : AWorldGenerationOption
{
	protected override string KeyName => "Seed_Normal";

	public override string ServerConfigName => null;

	public WorldSeedOption_Normal()
	{
		base.SpecialSeedNames = new string[0];
		base.SpecialSeedValues = new int[0];
		AWorldGenerationOption.OnOptionStateChanged += UpdateDependentState;
	}

	private void UpdateDependentState(AWorldGenerationOption changed){}
	protected override void OnEnabledStateChanged(){}
}
