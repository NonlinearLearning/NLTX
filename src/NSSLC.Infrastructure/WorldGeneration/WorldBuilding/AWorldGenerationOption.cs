using System;
using NSSLC.WorldGeneration.HostGraphics;
using NSSLC.WorldGeneration.GameContent.UI.Elements;
using NSSLC.WorldGeneration.Localization;
using NSSLC.WorldGeneration.UI;

namespace NSSLC.WorldGeneration.WorldBuilding;

public abstract class AWorldGenerationOption
{
	private bool _enabled;

	public bool AutoGenEnabled;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled != value)
			{
				_enabled = value;
				OnEnabledStateChanged();
				AWorldGenerationOption.OnOptionStateChanged(this);
			}
		}
	}

	protected abstract string KeyName { get; }

	public abstract string ServerConfigName { get; }

	public string[] SpecialSeedNames { get; protected set; }

	public int[] SpecialSeedValues { get; protected set; }

	public LocalizedText Description { get; private set; }

	public LocalizedText Title { get; private set; }

	protected Asset<Texture2D> Texture { get; private set; }

	protected static event Action<AWorldGenerationOption> OnOptionStateChanged;

	protected virtual void OnEnabledStateChanged()
{
	
	
	}
	public void Load()
{
    Description = Language.GetText("UI." + KeyName);
    Title = Language.GetText("UI." + KeyName + "_Title");
  }
	public virtual UIElement ProvideUIElement()
{
    throw new NotSupportedException("This world generator has no UI host.");
  }}
