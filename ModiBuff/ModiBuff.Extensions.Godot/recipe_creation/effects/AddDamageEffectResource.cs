using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class AddDamageEffectResource : EffectOnResource
	{
		[Export] public float AddDamage { get; set; }

		/// <summary>
		///		Can the added damage effect be reverted?
		///		Can the added damage effect be toggled on/off?
		/// </summary>
		[Export]
		public EffectState EffectState { get; set; }

		/// <summary>
		///		What should be done when the modifier gets stacked (ex. trigger effect, increase X value, etc). 
		/// </summary>
		[Export]
		public StackEffectType StackEffect { get; set; }

		public override IEffect GetEffect() => new AddDamageEffect(AddDamage, EffectState, StackEffect);
	}
}