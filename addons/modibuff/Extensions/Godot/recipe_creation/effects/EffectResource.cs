using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	/// <summary>
	///		DON'T USE this class directly, use any of the derived classes instead.
	/// </summary>
	[GlobalClass]
	public abstract partial class EffectResource : Resource, IEffectResource
	{
		/// <summary>
		///		Who should be the target and owner of the applied modifier. For further information, see <see cref="ModiBuff.Core.Targeting"/>
		/// </summary>
		[Export]
		public Targeting Targeting { get; set; }

		public abstract IEffect GetEffect();
	}

	public interface IEffectResource
	{
		IEffect GetEffect();
	}
}