namespace ModiBuff.Core
{
	public sealed class InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly bool _oneTime;
		private readonly ModifierCheck _modifierCheck;
		private readonly bool _check;

		private bool _isInitialized;

		public InitComponent(bool oneTimeInit, IEffect effect, ModifierCheck check) : this(oneTimeInit, new[] { effect }, check)
		{
		}

		public InitComponent(bool oneTimeInit, IEffect[] effects, ModifierCheck check)
		{
			_oneTime = oneTimeInit;
			_effects = effects;
			_modifierCheck = check;

			_check = check != null;
		}

		public void Init(IUnit target, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			if (_check && !_modifierCheck.Check(owner))
				return;

			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(target, owner);

			_isInitialized = true;
		}

		public void ResetState() => _isInitialized = false;
	}
}