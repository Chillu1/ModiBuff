using System.Collections.Generic;

namespace ModiBuff.Core
{
	public struct InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly IEffect[] _registerEffects;
		private readonly bool _oneTime;
		private readonly ModifierCheck _modifierCheck;

		private bool _isInitialized;

		public InitComponent(bool oneTimeInit, IEffect[] effects, ModifierCheck check)
		{
			_oneTime = oneTimeInit;
			_effects = effects;

			//TEMP
			var registerEffectsList = new List<IEffect>();
			for (int i = 0; i < effects.Length; i++)
			{
				var effect = effects[i];
				if (effect is IRegisterEffect)
					registerEffectsList.Add(effect);
			}

			if (registerEffectsList.Count > 0)
			{
				_registerEffects = registerEffectsList.ToArray();
				_effects = new IEffect[effects.Length - registerEffectsList.Count];
				int effectsIndex = 0;
				for (int i = 0; i < effects.Length; i++)
				{
					var effect = effects[i];
					if (effect is IRegisterEffect)
						continue;

					_effects[effectsIndex] = effect;
					effectsIndex++;
				}
			}
			else
				_registerEffects = null;

			_modifierCheck = check;

			_isInitialized = false;
		}

		public void Init(IUnit target, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			//TODO Not ideal, especially since init is called often, and effects will be duplicated in _effects array as well now
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(target, owner);

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(target, owner);

			_isInitialized = true;
		}

		public void Init(IList<IUnit> targets, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(targets, owner);

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(targets, owner);

			_isInitialized = true;
		}

		public void InitLoad(IUnit target, IUnit owner)
		{
			if (_registerEffects == null)
				return;

			for (int i = 0; i < _registerEffects.Length; i++)
				_registerEffects[i].Effect(target, owner);
		}

		public void InitLoad(IList<IUnit> targets, IUnit owner)
		{
			if (_registerEffects == null)
				return;

			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(targets, owner);
		}

		public void ResetState() => _isInitialized = false;

		public SaveData SaveState() => new SaveData(_isInitialized);
		public void LoadState(SaveData data) => _isInitialized = data.IsInitialized;

		public readonly struct SaveData
		{
			public readonly bool IsInitialized;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(bool isInitialized) => IsInitialized = isInitialized;
		}
	}
}