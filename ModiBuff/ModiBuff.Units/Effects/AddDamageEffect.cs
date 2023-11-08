namespace ModiBuff.Core.Units
{
	public sealed class AddDamageEffect : IStackEffect, IMutableStateEffect, IRevertEffect,
		IStackRevertEffect, IEffect, IModifierStateInfo<AddDamageEffect.Data>
	{
		public bool IsRevertible => _effectState.IsRevertible();
		public bool UsesMutableState => _effectState.IsRevertibleOrTogglable() || _stackEffect.UsesMutableState();

		private readonly float _damage;
		private readonly EffectState _effectState;
		private readonly StackEffectType _stackEffect;
		private readonly float _stackValue;
		private readonly Targeting _targeting;

		private bool _isEnabled;
		private float _extraDamage;
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, EffectState effectState = EffectState.None,
			StackEffectType stackEffect = StackEffectType.Effect, float stackValue = -1,
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
			StackEffectType stackEffect = StackEffectType.Effect, float stackValue = -1,
			Targeting targeting = Targeting.TargetSource) =>
			new AddDamageEffect(damage, effectState, stackEffect, stackValue, targeting);

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			if (!(target is IAddDamage<float> addDamage))
				return;

			if (_effectState.IsTogglable())
			{
				if (_isEnabled)
					return;

				_isEnabled = true;
			}

			float damage = _damage + _extraDamage;

			if (IsRevertible)
				_totalAddedDamage += damage;

			addDamage.AddDamage(damage);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			_targeting.UpdateTarget(ref target, source);
			if (!(target is IAddDamage<float> addDamage))
				return;

			if (_effectState.IsTogglable())
				_isEnabled = false;

			addDamage.AddDamage(-_totalAddedDamage);
			_totalAddedDamage = 0;
		}

		public void StackEffect(int stacks, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += _stackValue;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += _stackValue * stacks;

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
					addDamageTarget.AddDamage(-_damage - _extraDamage);
			}

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage -= _stackValue * stacks;

			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage -= _stackValue;
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
	}
}