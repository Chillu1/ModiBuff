using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	/// <summary>
	///		Applies another modifier to the target.
	/// </summary>
	[GlobalClass]
	public sealed partial class ApplierEffectResource : EffectOnResource
	{
		/// <summary>
		///		Name of the modifier that should be applied.
		/// </summary>
		[Export]
		public string ModifierName { get; set; }

		public override IEffect GetEffect() => new ApplierEffect(ModifierName);
	}
}