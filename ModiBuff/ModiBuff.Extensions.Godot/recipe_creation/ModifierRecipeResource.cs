using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class ModifierRecipeResource : BaseModifierRecipeResource
	{
		private float _applyChance;
		private float _effectChance;

		//---ApplyChecks---

		//Apply checks are different than effect checks, in a way that
		//apply checks are checked before the modifier is applied to the target.
		//While effect checks happen when the modifier is applied to the target, and check the target instead of the owner.

		/// <summary>
		///		For a modifier to be applied, the owner must be able to perform an action (ex. attack, cast, move).
		/// </summary>
		/// <example> A modifier can't work if the owner can't attack or move. </example>
		[ExportGroup("ApplyConditions")]
		[ExportSubgroup("LegalAction ApplyCondition")]
		[Export]
		public LegalAction LegalActionApplyCondition { get; set; }

		/// <summary>
		///		For a modifier to be applied, the owner must be in a specific state (ex. stunned, rooted, disarmed, silenced).
		/// </summary>
		[ExportSubgroup("StatusEffect ApplyCondition")]
		[Export]
		public StatusEffectType StatusEffectApplyCondition { get; set; }

		/// <summary>
		///		For a modifier to be applied, the owner must be in a specific state (ex. full health).
		/// </summary>
		[ExportSubgroup("")]
		[Export]
		public ConditionType ApplyCondition { get; set; }

		/// <summary>
		///		For a modifier to be applied, the owner must be in a specific state (ex. full health).
		/// </summary>
		[Export]
		public StatApplyConditionResource StatApplyCondition { get; set; }

		/// <summary>
		///		For a modifier to be applied, the owner must have a modifier with <see cref="ModifierNameApplyCondition"/> name.
		/// </summary>
		[Export]
		public string ModifierNameApplyCondition { get; set; }

		/// <summary>
		///		Cooldown set for when we can try to apply the modifier to a target.
		/// </summary>
		[Export]
		public float ApplyCooldown { get; set; }

		/// <summary>
		///		Cost for when we can trying to apply the modifier to a target, ex. through casting.
		/// </summary>
		[Export]
		public ApplyCostResource ApplyCost { get; set; }

		/// <summary>
		///		When trying to apply a modifier, what should the chance be of it being applied? Value between or equal to 0 and 1.
		/// </summary>
		[Export(PropertyHint.Range, "0,1,0.05")]
		public float ApplyChance
		{
			get => _applyChance;
			set
			{
				if (value >= 1)
					value /= 100f;
				_applyChance = value;
			}
		}

		//---EffectChecks---

		//Effect checks are different than apply checks, in a way that
		//effect checks are checked when the modifier is already applied to the target.
		//While apply checks happen before the modifier is applied to the target, and check the owner instead of the target.

		/// <summary>
		///		To trigger the modifier effects, the owner must be able to perform an action (ex. attack, cast, move).
		/// </summary>
		/// <example> A modifier can't work if the owner can't attack or move. </example>
		[ExportGroup("Effect Conditions")]
		[ExportSubgroup("LegalAction EffectCondition")]
		[Export]
		public LegalAction LegalActionEffectCondition { get; set; }

		/// <summary>
		///		To trigger the modifier effects, the owner must be in a specific state (ex. stunned, rooted, disarmed, silenced).
		/// </summary>
		[ExportSubgroup("StatusEffect EffectCondition")]
		[Export]
		public StatusEffectType StatusEffectEffectCondition { get; set; }

		/// <summary>
		///		To trigger the modifier effects, the owner must be in a specific state (ex. full health).
		/// </summary>
		[ExportSubgroup("")]
		[Export]
		public ConditionType EffectCondition { get; set; }

		/// <summary>
		///		To trigger the modifier effects, the owner must be in a specific state (ex. full health).
		/// </summary>
		[Export]
		public StatApplyConditionResource StatEffectCondition { get; set; }

		/// <summary>
		///		To trigger the modifier effects, the owner must have a modifier with <see cref="ModifierNameApplyCondition"/> name.
		/// </summary>
		[Export]
		public string ModifierNameEffectCondition { get; set; }

		/// <summary>
		///		Cooldown set for when we can try to trigger the effects.
		///		This works like a global cooldown.
		///		Each entity that tries to apply this modifier on another entity will have to wait for the cooldown.
		/// </summary>
		[Export]
		public float EffectCooldown { get; set; }

		/// <summary>
		///		Cost for when we can trying to trigger the effects to a target.
		///		This will take the cost from the target, not the owner.
		///		Can't work if the target doesn't have enough resources.
		/// </summary>
		[Export]
		public ApplyCostResource EffectCost { get; set; }

		/// <summary>
		///		To trigger the modifier effects, what should the chance be of it being applied? Value between or equal to 0 and 1.
		/// </summary>
		[Export(PropertyHint.Range, "0,1,0.05")]
		public float EffectChance
		{
			get => _effectChance;
			set
			{
				if (value >= 1)
					value /= 100f;
				_effectChance = value;
			}
		}

		//---Effects---

		/// <summary>
		///		Effects that get applied on specific actions (init, stack, interval, duration). 
		/// </summary>
		[ExportGroup("")] //[ExportGroup("Effects")]
		[Export]
		public EffectOnResource[] EffectResources { get; set; }

		//---Actions---

		/// <summary>
		///		When adding a modifier, trigger the Init effects only once (further adding of the same modifier will not trigger the init effects).
		/// </summary>
		/// <remarks>Works well for auras</remarks>
		[ExportGroup("")] //[ExportGroup("Actions")]
		[Export]
		public bool OneTimeInit { get; set; }

		/// <summary>
		///		How many seconds should pass between the interval effects get applied.
		/// </summary>
		/// <example> 0.5 = two times per second </example>
		[Export]
		public float Interval { get; set; }

		/// <summary>
		///		How many seconds should pass before the duration effects get triggered (usually modifier removal).
		/// </summary>
		[Export]
		public float Duration { get; set; }

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		/// <remarks> Same as <see cref="Duration"/> value </remarks>
		[Export]
		public float RemoveDuration { get; set; }

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the interval or duration be reset?
		/// </summary>
		/// <remarks> This is most often used to refresh duration of the modifier, like refreshing DoT modifiers </remarks>
		[Export]
		public RefreshType RefreshType { get; set; }

		/// <summary>
		///		Adds stack functionality to the modifier. A stack is added every time the modifier gets re-added to the target.
		/// </summary>
		[Export]
		public StackResource StackResource { get; set; }

		public override bool Validate()
		{
			bool valid = base.Validate();

			if (Duration != 0 && RemoveDuration != 0)
			{
				valid = false;
				GD.PushError("Recipe has both duration and remove duration set, " +
				             "if you want to remove the modifier after a duration, only use RemoveDuration");
			}

			return valid;
		}
	}
}