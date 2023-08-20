using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class HealActionEffectResource : EffectOnResource
	{
		public override IEffect GetEffect() => new HealActionEffect();
	}
}