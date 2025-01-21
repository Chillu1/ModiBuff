using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class DamageEffectResource : EffectOnResource
	{
		[Export] public float Damage { get; set; }

		/// <summary>
		///		What should be done when the modifier gets stacked (ex. trigger effect, increase X value, etc). 
		/// </summary>
		[Export]
		public StackEffectType StackEffect { get; set; }

		public override IEffect GetEffect() => new DamageEffect(Damage, false, StackEffect);
	}
}