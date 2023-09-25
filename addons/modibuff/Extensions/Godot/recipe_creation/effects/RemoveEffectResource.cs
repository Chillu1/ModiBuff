using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class RemoveEffectResource : EffectOnResource
	{
		public override IEffect GetEffect() => new RemoveEffect();
	}
}