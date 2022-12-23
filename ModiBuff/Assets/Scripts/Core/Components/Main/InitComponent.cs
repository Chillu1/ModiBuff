namespace ModiBuff.Core
{
	public class InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly bool _oneTime;
		private bool _isInitialized;

		public InitComponent(bool oneTimeInit, IEffect effect) : this(oneTimeInit, new[] { effect })
		{
		}

		public InitComponent(bool oneTimeInit, IEffect[] effects)
		{
			_oneTime = oneTimeInit;
			_effects = effects;
		}

		public void Init(IUnit target, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(target, owner);

			_isInitialized = true;
		}

		public void ResetState() => _isInitialized = false;
	}
}