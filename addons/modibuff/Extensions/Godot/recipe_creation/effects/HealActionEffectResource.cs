using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class HealActionEffectResource : EffectOnResource
	{
		public override IEffect GetEffect() => new HealActionEffect();
	}
}