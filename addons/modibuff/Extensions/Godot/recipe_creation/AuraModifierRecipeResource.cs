using Godot;

namespace ModiBuff.Extensions.Godot
{
	//Limited for now on purpose, since not all modifier functionality works
	[GlobalClass]
	public sealed partial class AuraModifierRecipeResource : BaseModifierRecipeResource
	{
		/// <summary>
		///		Tied aura effects, use this if we're using an applier as a EffectResource 
		/// </summary>
		[Export]
		public AuraEffectModifierRecipeResource[] AuraEffectResources { get; set; }

		//---Effects---

		/// <summary>
		///		Effects that get applied on specific actions (init, stack, interval, duration). 
		/// </summary>
		[Export]
		public EffectOnResource[] EffectResources { get; set; }

		//---Actions---

		/// <summary>
		///		How many seconds should pass between the interval effects get applied.
		/// </summary>
		/// <example> 0.5 = two times per second </example>
		[Export]
		public float Interval { get; set; }

		public override bool Validate()
		{
			bool valid = base.Validate();

			if ((AuraEffectResources == null || AuraEffectResources.Length == 0) &&
			    (EffectResources == null || EffectResources.Length == 0))
				GD.PushError($"Aura Recipe {Name} has no effects");

			if (AuraEffectResources != null)
			{
				if (AuraEffectResources.Length == 0)
					GD.PushError($"Aura Recipe {Name} has no aura effects, but isn't null");
				else
					foreach (var auraEffectModifierRecipeResource in AuraEffectResources)
						valid &= auraEffectModifierRecipeResource.Validate();
			}

			return valid;
		}
	}
}