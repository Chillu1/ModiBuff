using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class StatusEffectEffectResource : EffectOnResource
	{
		/// <summary>
		///		What status effect should be applied (ex. Stun, Root, etc).
		/// </summary>
		[Export]
		public StatusEffectType StatusEffectType { get; set; }

		/// <summary>
		///		Duration of the status effect.
		/// </summary>
		[Export]
		public float Duration { get; set; }

		/// <summary>
		///		Should the status effect be revertible?
		/// </summary>
		[Export]
		public bool IsRevertible { get; set; }

		/// <summary>
		///		What should be done when the modifier gets stacked (ex. trigger effect, increase X value, etc). 
		/// </summary>
		[Export]
		public StackEffectType StackEffect { get; set; }

		public override IEffect GetEffect() => new StatusEffectEffect(StatusEffectType, Duration, IsRevertible, StackEffect);
	}
}