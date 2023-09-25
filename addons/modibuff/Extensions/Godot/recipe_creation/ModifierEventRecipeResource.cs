using Godot;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class ModifierEventRecipeResource : BaseModifierRecipeResource
	{
		/// <summary>
		///		What unit event should trigger the modifier.
		/// </summary>
		/// <example> <see cref="Core.Units.EffectOnEvent.WhenAttacked"/> </example>
		/// <example> <see cref="EffectOnEvent.OnAttack"/> </example>
		[Export]
		public EffectOnEvent EffectOnEvent { get; set; }

		/// <summary>
		///		Effects that get applied on event triggers.
		/// </summary>
		[Export]
		public EffectResource[] EffectResources { get; set; }

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		[Export]
		public float RemoveDuration { get; set; }

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the duration be reset?
		/// </summary>
		[Export]
		public bool Refresh { get; set; }

		public override bool Validate()
		{
			bool valid = base.Validate();

			if (RemoveDuration == 0 && Refresh)
			{
				valid = false;
				GD.PushError("Recipe has refresh set but no remove duration");
			}

			return valid;
		}
	}
}