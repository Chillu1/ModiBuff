using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class AttackActionEffectResource : EffectOnResource
	{
		public override IEffect GetEffect() => new AttackActionEffect();
	}
}