namespace ModiBuff.Core.Units
{
	public sealed class AddDamageEffect : ITargetEffect, IStackEffect, IStateEffect, IRevertEffect,
		IEffect, IModifierStateInfo<AddDamageEffect.Data>
	{
		public bool IsRevertible { get; }

		private readonly float _damage;
		private readonly StackEffectType _stackEffect;
		private readonly bool _isTogglable; //TODO Needs to be revertible to be togglable
		private Targeting _targeting;

		private bool _isEnabled;
		private float _extraDamage;
		private float _totalAddedDamage;

		public AddDamageEffect(float damage, bool revertible = false, bool togglable = false,
			StackEffectType stackEffect = StackEffectType.Effect) : this(damage, revertible, togglable, stackEffect,
			Targeting.TargetSource)
		{
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static AddDamageEffect Create(float damage, bool revertible = false, bool togglable = false,
			StackEffectType stackEffect = StackEffectType.Effect, Targeting targeting = Targeting.TargetSource) =>
			new AddDamageEffect(damage, revertible, togglable, stackEffect, targeting);

		private AddDamageEffect(float damage, bool revertible, bool togglable, StackEffectType stackEffect,
			Targeting targeting)
		{
			_damage = damage;
			IsRevertible = revertible;
			_isTogglable = togglable;
			_stackEffect = stackEffect;
			_targeting = targeting;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit source)
		{
			if (_isTogglable)
			{
				if (_isEnabled)
					return;

				_isEnabled = true;
			}

			if (IsRevertible)
				_totalAddedDamage += _damage + _extraDamage;

			_targeting.UpdateTarget(ref target, source);
			((IAddDamage<float>)target).AddDamage(_damage + _extraDamage);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			if (_isTogglable)
				_isEnabled = false;

			_targeting.UpdateTarget(ref target, source);
			((IAddDamage<float>)target).AddDamage(-_totalAddedDamage);
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_extraDamage += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_extraDamage += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public Data GetEffectData() => new Data(_damage, _extraDamage);

		public void ResetState()
		{
			_isEnabled = false;
			_extraDamage = 0;
			_totalAddedDamage = 0;
		}

		public IEffect ShallowClone() =>
			new AddDamageEffect(_damage, IsRevertible, _isTogglable, _stackEffect, _targeting);

		object IShallowClone.ShallowClone() => ShallowClone();

		public struct Data
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