namespace ModiBuff.Core.Units
{
	public sealed class StatusEffectEffect : IStateEffect, IStackEffect, IRevertEffect, IEffect,
		IModifierIdOwner, IModifierGenIdOwner, IEffectStateInfo<StatusEffectEffect.Data>,
		ISavableEffect<StatusEffectEffect.SaveData>
	{
		public bool IsRevertible { get; }

		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;
		private readonly StackEffectType _stackEffect;
		private readonly float? _stackValue;
		private int _id;
		private int _genId;

		private float _extraDuration;
		private float _totalDuration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible = false,
			StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null) :
			this(statusEffectType, duration, revertible, stackEffect, stackValue, -1, -1)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static StatusEffectEffect Create(int id, int genId, StatusEffectType statusEffectType, float duration,
			bool revertible = false, StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null) =>
			new StatusEffectEffect(statusEffectType, duration, revertible, stackEffect, stackValue, id, genId);

		private StatusEffectEffect(StatusEffectType statusEffectType, float duration, bool revertible,
			StackEffectType stackEffect, float? stackValue, int id, int genId)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
			IsRevertible = revertible;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
			_id = id;
			_genId = genId;
		}

		public void SetModifierId(int id) => _id = id;
		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
			if (target is not IStatusEffectOwner<LegalAction, StatusEffectType> statusEffectTarget)
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IStatusEffectOwner<LegalAction, StatusEffectType>));
#endif
				return;
			}

			if (IsRevertible)
				_totalDuration = _duration + _extraDuration;
			statusEffectTarget.StatusEffectController.ChangeStatusEffect(_id, _genId, _statusEffectType,
				_duration + _extraDuration, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (target is not IStatusEffectOwner<LegalAction, StatusEffectType> statusEffectTarget)
				return;

			statusEffectTarget.StatusEffectController.DecreaseStatusEffect(_id, _genId, _statusEffectType,
				_totalDuration, source);

			_totalDuration = 0;
		}

		public Data GetEffectData() => new Data(_duration, _extraDuration);

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDuration += _stackValue!.Value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDuration += _stackValue!.Value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraDuration = 0;
			_totalDuration = 0;
		}

		public IEffect ShallowClone() => new StatusEffectEffect(_statusEffectType, _duration,
			IsRevertible, _stackEffect, _stackValue, _id, _genId);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_extraDuration, _totalDuration);

		public void LoadState(object state)
		{
			var saveData = (SaveData)state;
			_extraDuration = saveData.ExtraDuration;
			_totalDuration = saveData.TotalDuration;
		}

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

		public readonly struct SaveData
		{
			public readonly float ExtraDuration;
			public readonly float TotalDuration;

			public SaveData(float extraDuration, float totalDuration)
			{
				ExtraDuration = extraDuration;
				TotalDuration = totalDuration;
			}
		}
	}
}