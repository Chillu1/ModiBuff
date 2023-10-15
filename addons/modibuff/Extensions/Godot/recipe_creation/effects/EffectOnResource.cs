using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	/// <summary>
	///		DON'T USE this class directly, use any of the derived classes instead.
	/// </summary>
	[GlobalClass]
	public abstract partial class EffectOnResource : EffectResource //TODO Rename
	{
		/// <summary>
		///		When the effect should trigger (init, stack, interval, duration). Can be multiple.
		/// </summary>
		[Export]
		public EffectOn EffectOn { get; set; }
	}
}