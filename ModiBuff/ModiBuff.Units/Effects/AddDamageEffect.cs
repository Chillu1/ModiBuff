namespace ModiBuff.Core.Units
{
	public sealed class AddDamageEffect : IStackEffect, IMutableStateEffect, IRevertEffect,
		IStackRevertEffect, IEffect, IEffectStateInfo<AddDamageEffect.Data>,
		ISavableEffect<AddDamageEffect.SaveData>, ISaveableRecipeEffect<AddDamageEffect.RecipeSaveData>
	{
		public bool IsRevertible => EffectStateExtensions.HasFlag(_effectState, EffectState.IsRevertible);
		public bool IsStackRevertible => EffectStateExtensions.HasFlag(_effectState, EffectState.ValueIsRevertible);

		public bool UsesMutableState => EffectStateExtensions.HasFlag(_effectState, EffectState.IsRevertible) ||
		                                EffectStateExtensions.HasFlag(_effectState, EffectState.IsTogglable) ||
		                                _stackEffect.UsesMutableState();

		public bool UsesMutableStackEffect => _stackEffect.UsesMutableState();

		private readonly float _damage;
		private readonly EffectState _effectState;
		private readonly StackEffectType _stackEffect;
		private readonly float? _stackValue;
		private readonly Targeting _targeting;

		private bool _isEnabled;
		private float _extraDamage;
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, EffectState effectState = EffectState.None,
			StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null,
			Targeting targeting = Targeting.TargetSource)
		{
			_damage = damage;
			_effectState = effectState;
			_stackEffect = stackEffect;
			_stackValue = stackValue;
			_targeting = targeting;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static AddDamageEffect Create(float damage, EffectState effectState = EffectState.None,
			StackEffectType stackEffect = StackEffectType.Effect, float? stackValue = null,
			Targeting targeting = Targeting.TargetSource) =>
			new AddDamageEffect(damage, effectState, stackEffect, stackValue, targeting);

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			if (target is not IAddDamage<float> addDamageTarget)
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IAddDamage<float>));
#endif
				return;
			}

			if (EffectStateExtensions.HasFlag(_effectState, EffectState.IsTogglable))
			{
				if (_isEnabled)
					return;

				_isEnabled = true;
			}

			float damage = _damage + _extraDamage;

			if (IsRevertible)
				_totalAddedDamage += damage;

			addDamageTarget.AddDamage(damage);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			if (target is not IAddDamage<float> addDamage)
				return;

			if (EffectStateExtensions.HasFlag(_effectState, EffectState.IsTogglable))
			{
				if (!_isEnabled)
					return;

				_isEnabled = false;
			}

			//Might want to have a special method for reverting stats state, to not trigger events
			addDamage.AddDamage(-_totalAddedDamage);
			_totalAddedDamage = 0;
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue!.Value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += _stackValue!.Value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void RevertStack(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Effect) != 0)
			{
				_totalAddedDamage -= _damage + _extraDamage;

				_targeting.UpdateTarget(ref target, source);
				if (target is IAddDamage<float> addDamageTarget)
					//Might want to have a special method for reverting stats state, to not trigger events 
					addDamageTarget.AddDamage(-_damage - _extraDamage);
			}

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage -= _stackValue!.Value * stacks;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage -= _stackValue!.Value;
		}

		public Data GetEffectData() => new Data(_damage, _extraDamage);

		public void ResetState()
		{
			_isEnabled = false;
			_extraDamage = 0;
			_totalAddedDamage = 0;
		}

		public IEffect ShallowClone() =>
			new AddDamageEffect(_damage, _effectState, _stackEffect, _stackValue, _targeting);

		object IShallowClone.ShallowClone() => ShallowClone();

		public object SaveState() => new SaveData(_isEnabled, _extraDamage, _totalAddedDamage);

		public void LoadState(object saveData)
		{
			var data = (SaveData)saveData;
			_isEnabled = data.IsEnabled;
			_extraDamage = data.ExtraDamage;
			_totalAddedDamage = data.TotalAddedDamage;
		}

		public object SaveRecipeState() =>
			new RecipeSaveData(_damage, _effectState, _stackEffect, _stackValue, _targeting);

		public readonly struct Data
		{
			public readonly float BaseDamage;
			public readonly float ExtraDamage;

			public Data(float baseDamage, float extraDamage)
			{
				BaseDamage = baseDamage;
				ExtraDamage = extraDamage;
			}
		}

		public readonly struct SaveData
		{
			public readonly bool IsEnabled;
			public readonly float ExtraDamage;
			public readonly float TotalAddedDamage;

			public SaveData(bool isEnabled, float extraDamage, float totalAddedDamage)
			{
				IsEnabled = isEnabled;
				ExtraDamage = extraDamage;
				TotalAddedDamage = totalAddedDamage;
			}
		}

		public readonly struct RecipeSaveData
		{
			public readonly float Damage;
			public readonly EffectState EffectState;
			public readonly StackEffectType StackEffect;
			public readonly float? StackValue;
			public readonly Targeting Targeting;

			public RecipeSaveData(float damage, EffectState effectState, StackEffectType stackEffect,
				float? stackValue, Targeting targeting)
			{
				Damage = damage;
				EffectState = effectState;
				StackEffect = stackEffect;
				StackValue = stackValue;
				Targeting = targeting;
			}
		}
	}
}