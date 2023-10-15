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
		///		When adding a modifier, trigger the Init effects only once (further adding of the same modifier will not trigger the init effects).
		///		Use this when the aura effect should only be applied once. Example adding damage to units around the caster.
		/// </summary>
		[Export]
		public bool OneTimeInit { get; set; }

		/// <summary>
		///		How many seconds should pass before the aura effect modifier gets removed. Should be a bit longer than aura interval.
		/// </summary>
		[Export]
		public float RemoveDuration { get; set; }
	}
}