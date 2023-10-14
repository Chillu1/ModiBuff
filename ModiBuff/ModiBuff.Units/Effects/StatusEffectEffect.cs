namespace ModiBuff.Core.Units
{
	public sealed class StatusEffectEffect : IStateEffect, IStackEffect, IRevertEffect, IEffect,
		IModifierIdOwner, IModifierGenIdOwner, IModifierStateInfo<StatusEffectEffect.Data>
	{
		public bool IsRevertible { get; }

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;
		private int _id;
		private int _genId;

		private float _extraDuration;
		private float _totalDuration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible = false,
			StackEffectType stackEffect = StackEffectType.Effect) :
			this(statusEffectType, duration, revertible, stackEffect, -1, -1)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static StatusEffectEffect Create(int id, int genId, StatusEffectType statusEffectType, float duration,
			bool revertible = false, StackEffectType stackEffect = StackEffectType.Effect) =>
			new StatusEffectEffect(statusEffectType, duration, revertible, stackEffect, id, genId);

		private StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible,
			StackEffectType stackEffect, int id, int genId)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
			_id = id;
			_genId = genId;
		}

		public void SetModifierId(int id) => _id = id;
		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == -1)
				Logger.LogError("ModifierId is not set for status effect effect.");
			if (_genId == -1)
				Logger.LogError("GenId is not set for status effect effect.");
#endif

			if (IsRevertible)
				_totalDuration = _duration + _extraDuration;
			((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.ChangeStatusEffect(_id, _genId, _statusEffectType, _duration + _extraDuration);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == -1)
				Logger.LogError("ModifierId is not set for status effect effect.");
			if (_genId == -1)
				Logger.LogError("GenId is not set for status effect effect.");
#endif

			((IStatusEffectOwner<LegalAction, StatusEffectType>)target).StatusEffectController
				.DecreaseStatusEffect(_id, _genId, _statusEffectType, _totalDuration);
		}

		public Data GetEffectData() => new Data(_duration, _extraDuration);

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDuration += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDuration += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraDuration = 0;
			_totalDuration = 0;
		}

		public IEffect ShallowClone() =>
			new StatusEffectEffect(_statusEffectType, _duration, IsRevertible, _stackEffect, _id, _genId);

		object IShallowClone.ShallowClone() => ShallowClone();

		public readonly struct Data
		{
			public readonly float Duration;
			public readonly float ExtraDuration;

			public Data(float duration, float extraDuration)
			{
				Duration = duration;
				ExtraDuration = extraDuration;
			}
		}
	}
}