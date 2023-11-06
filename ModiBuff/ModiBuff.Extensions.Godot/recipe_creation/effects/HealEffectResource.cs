using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class HealEffectResource : EffectOnResource
	{
		[Export] public float Heal { get; set; }

		/// <summary>
		///		Can the heal effect be reverted?
		/// </summary>
		[Export]
		public HealEffect.EffectState EffectState { get; set; }

		/// <summary>
		///		What should be done when the modifier gets stacked (ex. trigger effect, increase X value, etc). 
		/// </summary>
		[Export]
		public StackEffectType StackEffect { get; set; }

		public override IEffect GetEffect() => new HealEffect(Heal, EffectState, StackEffect);
	}
}