using Godot;

namespace ModiBuff.Extensions.Godot
{
	//Limited for now on purpose, since not all modifier functionality works
	[GlobalClass]
	public sealed partial class AuraEffectModifierRecipeResource : BaseModifierRecipeResource
	{
		/// <summary>
		///		Effects that get applied on specific actions (init, stack, interval, duration). 
		/// </summary>
		[Export]
		public EffectOnResource[] EffectResources { get; set; }

		/// <summary>
		///		How many seconds should pass before the aura effect modifier gets removed. Should be a bit longer than aura interval.
		/// </summary>
		[Export]
		public float RemoveDuration { get; set; }
	}
}