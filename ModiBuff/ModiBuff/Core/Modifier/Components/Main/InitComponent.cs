using System.Collections.Generic;

namespace ModiBuff.Core
{
	public struct InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly bool _oneTime;
		private readonly ModifierCheck _modifierCheck;

		private bool _isInitialized;

		public InitComponent(bool oneTimeInit, IEffect[] effects, ModifierCheck check)
		{
			_oneTime = oneTimeInit;
			_effects = effects;
			_modifierCheck = check;

			_isInitialized = false;
		}

		public void Init(IUnit target, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(target, owner);

			_isInitialized = true;
		}

		public void Init(IList<IUnit> targets, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(targets, owner);

			_isInitialized = true;
		}

		public void ResetState() => _isInitialized = false;
	}
}